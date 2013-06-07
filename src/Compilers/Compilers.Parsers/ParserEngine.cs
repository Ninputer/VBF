using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Parsers.Generator;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class ParserEngine
    {
        private TransitionTable m_transitions;
        private ReduceVisitor m_reducer;

        private List<ParserHead> m_heads;
        private List<ParserHead> m_shiftedHeads;
        private List<ParserHead> m_reducedHeads;
        private List<ParserHead> m_recoverReducedHeads;
        private List<ParserHead> m_tempHeads;
        private List<ParserHead> m_errorCandidates;
        private List<ParserHead> m_acceptedHeads;

        ParserHeadCleaner m_cleaner;

        private SyntaxErrors m_errorDef;

        public int CurrentStackCount
        {
            get
            {
                return m_heads.Count;
            }
        }

        public int AcceptedCount
        {
            get
            {
                return m_acceptedHeads.Count;
            }
        }

        public object GetResult(int index, CompilationErrorManager errorManager)
        {
            CodeContract.RequiresArgumentInRange(index >= 0 && index < m_acceptedHeads.Count, "index", "index is out of range");

            var head = m_acceptedHeads[index];

            if (head.Errors != null && errorManager != null)
            {
                //aggregate errors
                foreach (var error in head.Errors)
                {
                    int errorId = error.ErrorId ?? m_errorDef.OtherErrorId;

                    errorManager.AddError(errorId, error.ErrorPosition, error.ErrorArgument);
                }
            }

            return head.TopStackValue;
        }

        public ResultInfo GetResultInfo(int index)
        {
            return new ResultInfo(m_acceptedHeads[index].Errors.Count);
        }

        public ParserEngine(TransitionTable transitions, SyntaxErrors errorDef)
        {
            CodeContract.RequiresArgumentNotNull(transitions, "transitions");
            CodeContract.RequiresArgumentNotNull(errorDef, "errorDef");

            m_transitions = transitions;
            m_reducer = new ReduceVisitor(transitions);

            m_heads = new List<ParserHead>();
            m_shiftedHeads = new List<ParserHead>();
            m_acceptedHeads = new List<ParserHead>();
            m_errorCandidates = new List<ParserHead>();
            m_tempHeads = new List<ParserHead>();
            m_reducedHeads = new List<ParserHead>();
            m_recoverReducedHeads = new List<ParserHead>();

            m_errorDef = errorDef;

            m_cleaner = new ParserHeadCleaner();

            //init state
            m_heads.Add(new ParserHead(new StackNode(0, null, null)));
        }

        public void Input(Lexeme z)
        {
            while (true)
            {
                var heads = m_heads;

                for (int i = 0; i < heads.Count; i++)
                {
                    var head = heads[i];

                    int stateNumber = head.TopStackStateIndex;


                    bool isShiftedOrReduced = false;

                    var shiftLexer = m_transitions.GetLexersInShifting(stateNumber);

                    int tokenIndex;
                    if (shiftLexer == null)
                    {
                        tokenIndex = z.TokenIndex;
                    }
                    else
                    {
                        tokenIndex = z.GetTokenIndex(shiftLexer.Value);
                    }

                    //get shift
                    var shifts = m_transitions.GetShift(stateNumber, tokenIndex);

                    //shifts
                    var shift = shifts;

                    while (shift != null)
                    {
                        isShiftedOrReduced = true;

                        var newHead = head.Clone();

                        newHead.Shift(z, shift.Value);

                        //save shifted heads
                        m_shiftedHeads.Add(newHead);

                        //get next shift
                        shift = shift.GetNext();
                    }



                    //reduces
                    var reduceLexer = m_transitions.GetLexersInReducing(stateNumber);

                    if (reduceLexer == null)
                    {
                        tokenIndex = z.TokenIndex;
                    }
                    else
                    {
                        tokenIndex = z.GetTokenIndex(reduceLexer.Value);
                    }

                    var reduces = m_transitions.GetReduce(stateNumber, tokenIndex);
                    var reduce = reduces;

                    while (reduce != null)
                    {
                        isShiftedOrReduced = true;

                        int productionIndex = reduce.Value;
                        IProduction production = m_transitions.NonTerminals[productionIndex];

                        var reducedHead = head.Clone();

                        reducedHead.Reduce(production, m_reducer, z);

                        if (reducedHead.IsAccepted)
                        {
                            m_acceptedHeads.Add(reducedHead);
                        }
                        else
                        {
                            //add back to queue, until shifted
                            m_reducedHeads.Add(reducedHead);                                
                        }

                        //get next reduce
                        reduce = reduce.GetNext();
                    }

                    if (!isShiftedOrReduced)
                    {
                        m_errorCandidates.Add(head);
                    }

                }
                
                if (m_reducedHeads.Count > 0)
                {
                    m_heads.Clear();
                    m_cleaner.CleanHeads(m_reducedHeads, m_heads);
                    m_reducedHeads.Clear();

                    continue;
                }
                else if (m_shiftedHeads.Count == 0 && m_acceptedHeads.Count == 0)
                {
                    //no action for current lexeme, error recovery
                    RecoverError(z);
                }
                else
                {
                    break;
                }
            }

            CleanShiftedAndAcceptedHeads();
        }

        private void RecoverError(Lexeme z)
        {
            List<ParserHead> shiftedHeads = m_shiftedHeads;

            m_heads.Clear();
            int errorHeadCount = m_errorCandidates.Count;

            Debug.Assert(errorHeadCount > 0);

            for (int i = 0; i < errorHeadCount; i++)
            {
                var head = m_errorCandidates[i];                

                //option 1: remove
                //remove current token and continue
                if (!z.IsEndOfStream)
                {
                    var deleteHead = head.Clone();

                    deleteHead.IncreaseErrorRecoverLevel();
                    deleteHead.AddError(new ErrorRecord(m_errorDef.TokenUnexpectedId, z.Value.Span) { ErrorArgument = z.Value });

                    shiftedHeads.Add(deleteHead);
                }

                //option 2: insert
                //insert all possible shifts token and continue
                Queue<ParserHead> recoverQueue = new Queue<ParserHead>();

                for (int j = 0; j < m_transitions.TokenCount - 1; j++)
                {
                    recoverQueue.Enqueue(head);

                    while (recoverQueue.Count > 0)
                    {
                        var recoverHead = recoverQueue.Dequeue();
                        int recoverStateNumber = recoverHead.TopStackStateIndex;

                        var shiftLexer = m_transitions.GetLexersInShifting(recoverStateNumber);

                        int tokenIndex;
                        if (shiftLexer == null)
                        {
                            tokenIndex = z.TokenIndex;
                        }
                        else
                        {
                            tokenIndex = z.GetTokenIndex(shiftLexer.Value);
                        }

                        var recoverShifts = m_transitions.GetShift(recoverStateNumber, j);
                        var recoverShift = recoverShifts;

                        while (recoverShift != null)
                        {
                            var insertHead = recoverHead.Clone();

                            var insertLexeme = z.GetErrorCorrectionLexeme(j, m_transitions.GetTokenDescription(j));
                            insertHead.Shift(insertLexeme, recoverShift.Value);
                            insertHead.IncreaseErrorRecoverLevel();
                            insertHead.AddError(new ErrorRecord(m_errorDef.TokenMissingId, z.Value.Span) { ErrorArgument = insertLexeme.Value });

                            m_heads.Add(insertHead);

                            recoverShift = recoverShift.GetNext();
                        }

                        var reduceLexer = m_transitions.GetLexersInReducing(recoverStateNumber);

                        if (reduceLexer == null)
                        {
                            tokenIndex = z.TokenIndex;
                        }
                        else
                        {
                            tokenIndex = z.GetTokenIndex(reduceLexer.Value);
                        }

                        var recoverReduces = m_transitions.GetReduce(recoverStateNumber, j);
                        var recoverReduce = recoverReduces;

                        while (recoverReduce != null)
                        {
                            int productionIndex = recoverReduce.Value;
                            IProduction production = m_transitions.NonTerminals[productionIndex];

                            var reducedHead = recoverHead.Clone();

                            reducedHead.Reduce(production, m_reducer, z);

                            //add back to queue, until shifted
                            m_recoverReducedHeads.Add(reducedHead);

                            //get next reduce
                            recoverReduce = recoverReduce.GetNext();
                        }

                        if (m_recoverReducedHeads.Count > 0)
                        {
                            m_tempHeads.Clear();
                            m_cleaner.CleanHeads(m_recoverReducedHeads, m_tempHeads);
                            m_recoverReducedHeads.Clear();

                            foreach (var recoveredHead in m_tempHeads)
                            {
                                recoverQueue.Enqueue(recoveredHead);
                            }
                        }
                    }
                }
            }
        }        

        private void CleanShiftedAndAcceptedHeads()
        {
            m_heads.Clear();
            m_errorCandidates.Clear();
            m_tempHeads.Clear();

            if (m_acceptedHeads.Count > 0)
            {
                m_cleaner.CleanHeads(m_acceptedHeads, m_tempHeads);
                m_acceptedHeads.Clear();

                var swap = m_tempHeads;
                m_tempHeads = m_acceptedHeads;
                m_acceptedHeads = swap;
            }            

            if (m_shiftedHeads.Count > 0)
            {
                m_cleaner.CleanHeads(m_shiftedHeads, m_heads);
                m_shiftedHeads.Clear();
            }
            
        }

    }
}
