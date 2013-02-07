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
    internal class ParserHead
    {
        private StackNode m_topStack;

        //Indicates current error recover level
        //level = 0 means not error
        //level = 1 means recovered 1 error, etc
        private int m_errorRecoverLevel = 0;
        private List<ErrorRecord> m_errors;

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

        public int ErrorRecoverLevel
        {
            get
            {
                return m_errorRecoverLevel;
            }
        }

        public void AddError(ErrorRecord error)
        {
            if (m_errors == null)
            {
                m_errors = new List<ErrorRecord>();
            }

            m_errors.Add(error);
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

        public void Reduce(IProduction production, ReduceVisitor reducer, Lexeme lookahead)
        {
            if (production == null)
            {
                //Accept
                Debug.Assert(m_topStack.PrevNode.StateIndex == 0);

                //TODO: accepted
                IsAccepted = true;
                return;
            }

            reducer.TopStack = m_topStack;
            reducer.ReduceError = null;
            production.Accept(reducer);

            m_topStack = reducer.NewTopStack;

            if (reducer.ReduceError != null)
            {
                IncreaseErrorRecoverLevel();

                if (reducer.ReduceError.ErrorPosition == null)
                {
                    reducer.ReduceError.ErrorPosition = lookahead.Span;
                }

                AddError(reducer.ReduceError);
            }
        }

        public void IncreaseErrorRecoverLevel()
        {
            ++m_errorRecoverLevel;
        }

        public ParserHead Clone()
        {
            return (ParserHead)MemberwiseClone();
        }
    }
}
