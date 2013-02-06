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

        public object GetResult(int index)
        {
            CodeContract.RequiresArgumentInRange(index >= 0 && index < m_acceptedHeads.Count, "index", "index is out of range");

            return m_acceptedHeads[index].TopStackValue;
        }

        public ParserDriver(TransitionTable transitions)
        {
            CodeContract.RequiresArgumentNotNull(transitions, "transitions");

            m_transitions = transitions;
            m_reducer = new ReduceVisitor(transitions);

            m_heads = new List<ParserHead>();
            m_tempHeads = new List<ParserHead>();
            m_acceptedHeads = new List<ParserHead>();

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

                    reducedHead.Reduce(production, m_reducer);

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
                    //TODO: no action for current lexeme, error recovery
                }
            }

            SwapAndClean();
        }

        private void SwapAndClean()
        {
            var temp = m_tempHeads;
            m_tempHeads = m_heads;
            m_heads = temp;

            m_tempHeads.Clear();
        }


    }
}
