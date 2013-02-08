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
        public int Priority { get; set; }

#if HISTORY
        public List<string> History = new List<string>();
#endif

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

        public IReadOnlyList<ErrorRecord> Errors
        {
            get
            {
                return m_errors;
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
#if HISTORY
            var from = m_topStack.StateIndex;
#endif

            StackNode shiftNode = new StackNode(targetStateIndex, m_topStack, z);            

            m_topStack = shiftNode;

#if HISTORY
            var to = m_topStack.StateIndex;
            History.Add(String.Format("S{0}:{1}", from, to));
#endif
        }

        public void Reduce(IProduction production, ReduceVisitor reducer, Lexeme lookahead)
        {
#if HISTORY
            var from = m_topStack.StateIndex;
#endif

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

            if (Priority < production.Priority) Priority = production.Priority;

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

#if HISTORY
            var to = m_topStack.StateIndex;
            History.Add(String.Format("R{0}:{1}", from, to));
#endif
        }

        public void IncreaseErrorRecoverLevel()
        {
            ++m_errorRecoverLevel;
        }

        public ParserHead Clone()
        {
            var newHead = (ParserHead)MemberwiseClone();

            if (m_errors != null)
            {
                newHead.m_errors = m_errors.ToList();
            }

#if HISTORY
            newHead.History = History.ToList();
#endif
            return newHead;
        }
    }
}
