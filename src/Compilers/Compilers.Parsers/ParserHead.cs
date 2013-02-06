using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Parsers.Generator;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    internal class ParserHead
    {
        private StackNode m_topStack;

        //Indicates current error recover level
        //level = 0 means not error
        //level = 1 means recovered 1 error, etc
        private int m_errorRecoverLevel;

        public bool IsAccepted { get; private set; }

        public int TopStackStateIndex
        {
            get
            {
                return m_topStack.StateIndex;
            }
        }

        public object TopStackValue
        {
            get
            {
                return m_topStack.ReducedValue;
            }
        }

        public ParserHead(StackNode topStack)
        {
            m_topStack = topStack;
            m_errorRecoverLevel = 0;
        }

        public void Shift(Lexeme z, int targetStateIndex)
        {
            StackNode shiftNode = new StackNode();
            shiftNode.ReducedValue = z;
            shiftNode.StateIndex = targetStateIndex;
            shiftNode.PrevNode = m_topStack;

            m_topStack = shiftNode;
        }

        public void Reduce(IProduction production, ReduceVisitor reducer)
        {
            if (production == null)
            {
                //Accept
                if (m_topStack.PrevNode.StateIndex != 0)
                {
                    throw new Exception("stack is not empty");
                }

                //TODO: accepted
                IsAccepted = true;
                return;
            }

            reducer.TopStack = m_topStack;
            production.Accept(reducer);

            m_topStack = reducer.NewTopStack;
        }

        public ParserHead Clone()
        {
            return new ParserHead(m_topStack);
        }
    }
}
