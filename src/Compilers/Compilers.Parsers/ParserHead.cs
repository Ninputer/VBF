using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Parsers.Generator;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class ParserHead
    {
        private StackNode m_topStack;
        private TransitionTable m_transitions;
        private ReduceVisitor m_reducer;
        private int m_eosIndex;

        public ParserHead(TransitionTable transitions)
        {
            CodeContract.RequiresArgumentNotNull(transitions, "transitions");

            m_eosIndex = transitions.EndOfStreamTokenIndex;
            m_topStack = new StackNode();

            //set init state
            m_topStack.StateIndex = 0;

            m_transitions = transitions;
            m_reducer = new ReduceVisitor(transitions);
        }

        public bool Input(Lexeme z)
        {
            int stateNumber = m_topStack.StateIndex;
            int tokenIndex = z.TokenIndex;

            //try shift
            var action = m_transitions.GetShift(stateNumber, tokenIndex);

            if (action != null)
            {
                //shift
                StackNode shiftNode = new StackNode();
                shiftNode.ReducedValue = z;
                shiftNode.StateIndex = action.Value;
                shiftNode.PrevNode = m_topStack;

                m_topStack = shiftNode;

                return true;
            }

            //try reduce
            var reduce = m_transitions.GetReduce(stateNumber, tokenIndex);

            if (reduce != null)
            {
                //reduce
                int productionIndex = reduce.Value;

                IProduction production = m_transitions.NonTerminals[productionIndex];

                if (production == null)
                {
                    //Accept
                    if (m_topStack.PrevNode.StateIndex != 0)
                    {
                        throw new Exception("stack is not empty");
                    }

                    //TODO: accepted
                    return true;
                }

                m_reducer.TopStack = m_topStack;
                production.Accept(m_reducer);

                m_topStack = m_reducer.NewTopStack;

                return false;
            }

            //error
            throw new Exception("Parsing has failed, unexpected token: " + z.Value);
        }
    }
}
