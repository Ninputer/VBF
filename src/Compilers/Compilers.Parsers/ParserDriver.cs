using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Parsers.Generator;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class ParserDriver
    {
        private TransitionTable m_transitions;
        private ReduceVisitor m_reducer;

        private List<ParserHead> m_heads;
        private List<ParserHead> m_tempHeads;
        private List<ParserHead> m_acceptedHeads;

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
                    int errorId = error.ErrorId ?? m_errorDef.OtherError;

                    errorManager.AddError(errorId, error.ErrorPosition, error.ErrorArgument);
                }
            }

            return head.TopStackValue;
        }

        public ParserDriver(TransitionTable transitions, SyntaxErrors errorDef)
        {
            CodeContract.RequiresArgumentNotNull(transitions, "transitions");
            CodeContract.RequiresArgumentNotNull(errorDef, "errorDef");

            m_transitions = transitions;
            m_reducer = new ReduceVisitor(transitions);

            m_heads = new List<ParserHead>();
            m_tempHeads = new List<ParserHead>();
            m_acceptedHeads = new List<ParserHead>();
            m_errorDef = errorDef;

            //init state
            m_heads.Add(new ParserHead(new StackNode()));
        }

        public void Input(Lexeme z)
        {
            List<ParserHead> shiftedHeads = m_tempHeads;

            int count = m_heads.Count;
            for (int i = 0; i < count; i++)
            {
                var head = m_heads[i];

                int stateNumber = head.TopStackStateIndex;
                int tokenIndex = z.TokenIndex;

                //get shift
                var shifts = m_transitions.GetShift(stateNumber, tokenIndex);

                //shifts
                var shift = shifts;

                while (shift != null)
                {
                    var newHead = head.Clone();

                    newHead.Shift(z, shift.Value);

                    //save shifted heads
                    shiftedHeads.Add(newHead);

                    //get next shift
                    shift = shift.GetNext();
                }

                //reduces
                var reduces = m_transitions.GetReduce(stateNumber, tokenIndex);

                var reduce = reduces;

                while (reduce != null)
                {
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
                        m_heads.Add(reducedHead);
                        ++count;
                    }

                    //get next reduce
                    reduce = reduce.GetNext();
                }


                if (shifts == null && reduces == null)
                {
                    //skip error recovery if there's already any accepted states
                    if (m_acceptedHeads.Count > 0)
                    {
                        continue;
                    }

                    //no action for current lexeme, error recovery

                    //option 1: remove
                    //remove current token and continue
                    if (!z.IsEndOfStream)
                    {
                        var deleteHead = head.Clone();

                        deleteHead.IncreaseErrorRecoverLevel();
                        deleteHead.AddError(new ErrorRecord(m_errorDef.TokenUnexpected, z.Span) { ErrorArgument = z.Value });

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

                            var recoverShifts = m_transitions.GetShift(recoverStateNumber, j);
                            var recoverShift = recoverShifts;

                            while (recoverShift != null)
                            {
                                var insertHead = recoverHead.Clone();

                                var insertLexeme = z.GetErrorCorrectionLexeme(j, m_transitions.GetTokenDescription(j));
                                insertHead.Shift(insertLexeme, recoverShift.Value);
                                insertHead.IncreaseErrorRecoverLevel();
                                insertHead.AddError(new ErrorRecord(m_errorDef.TokenMissing, z.Span) { ErrorArgument = insertLexeme.Value });

                                m_heads.Add(insertHead);
                                ++count;

                                recoverShift = recoverShift.GetNext();
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
                                recoverQueue.Enqueue(reducedHead);

                                //get next reduce
                                recoverReduce = recoverReduce.GetNext();
                            }
                        }
                    }
                }
            }

            SwapAndClean();
        }

        private List<ParserHead> CleanUpHeads(List<ParserHead> heads, List<ParserHead> comparedHeads)
        {
            var newHeads = new List<ParserHead>();

            var firstHead = heads[0];
            int minLevel = firstHead.ErrorRecoverLevel;
            int minError = firstHead.Errors != null ? firstHead.Errors.Count : 0;
            int maxPriority = 0;

            for (int i = 0; i < comparedHeads.Count; i++)
            {
                var head = comparedHeads[i];
                var headLevel = head.ErrorRecoverLevel;
                var priority = head.Priority;
                var errorCount = head.Errors != null ? head.Errors.Count : 0;

                if (minLevel > headLevel)
                {
                    minLevel = headLevel;
                }

                if (minError > errorCount)
                {
                    minError = errorCount;
                }

                if (maxPriority < priority)
                {
                    maxPriority = priority;
                }
            }

            //copy all heads with min error level, min error count and highest priority
            for (int i = 0; i < heads.Count; i++)
            {
                var head = heads[i];

                if (head.ErrorRecoverLevel == minLevel && head.Priority == maxPriority)
                {
                    if (head.Errors == null || head.Errors.Count == minError)
                    {
                        head.Priority = 0;
                        newHeads.Add(head);
                    }
                }
            }

            heads.Clear();

            return newHeads;
        }

        private void SwapAndClean()
        {
            //var temp = m_tempHeads;
            //m_tempHeads = m_heads;
            //m_heads = temp;

            //m_tempHeads.Clear();

            m_heads.Clear();

            CleanUpAcceptedHeads();

            if (m_tempHeads.Count == 0)
            {
                return;
            }

            //find min error level;
            int minLevel = m_tempHeads[0].ErrorRecoverLevel;
            int maxPriority = 0;

            for (int i = 0; i < m_tempHeads.Count; i++)
            {
                var head = m_tempHeads[i];
                var headLevel = head.ErrorRecoverLevel;
                var priority = head.Priority;

                if (minLevel > headLevel)
                {
                    minLevel = headLevel;
                }

                if (maxPriority < priority)
                {
                    maxPriority = priority;
                }
            }

            //copy all heads with min error level
            for (int i = 0; i < m_tempHeads.Count; i++)
            {
                var head = m_tempHeads[i];

                if (head.ErrorRecoverLevel == minLevel && head.Priority == maxPriority)
                {
                    head.Priority = 0;
                    m_heads.Add(head);
                }
            }

            m_tempHeads.Clear();
        }

        private void CleanUpAcceptedHeads()
        {
            if (m_acceptedHeads.Count > 0)
            {
                m_acceptedHeads = CleanUpHeads(m_acceptedHeads, m_acceptedHeads);
            }
        }


    }
}
