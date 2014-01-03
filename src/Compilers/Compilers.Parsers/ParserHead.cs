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
        private StackNode m_lastShiftStack;

        //Indicates current error recover level
        //level = 0 means not error
        //level = 1 means recovered 1 error, etc
        private int m_errorRecoverLevel = 0;
        private List<ErrorRecord> m_errors;

        public bool IsAccepted { get; private set; }
        public AmbiguityAggregator AmbiguityAggregator { get; set; }

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

            set
            {
                m_topStack.ReducedValue = value;
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

        public bool HasSameErrorsWith(ParserHead other)
        {
            if (other.m_errors == null && m_errors == null)
            {
                return true;
            }
            else if (other.m_errors == null || m_errors == null)
            {
                return false;
            }
            else
            {
                HashSet<ErrorRecord> myErrors = new HashSet<ErrorRecord>(m_errors);
                HashSet<ErrorRecord> otherErrors = new HashSet<ErrorRecord>(other.m_errors);

                return myErrors.SetEquals(otherErrors);
            }
        }

        public ParserHead(StackNode topStack)
        {
            m_topStack = topStack;
            m_lastShiftStack = topStack;
            m_errorRecoverLevel = 0;
        }

        public void Shift(Lexeme z, int targetStateIndex)
        {
#if HISTORY
            var from = m_topStack.StateIndex;
#endif

            StackNode shiftNode = new StackNode(targetStateIndex, m_topStack, z);

            m_topStack = shiftNode;
            m_lastShiftStack = shiftNode;

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

            if (production.AggregatesAmbiguities)
            {
                AmbiguityAggregator = (production as ProductionBase).CreateAggregator();
            }

            var reduceResult = production.Accept(reducer, m_topStack);

            m_topStack = reduceResult.NewTopStack;
            var reduceError = reduceResult.ReduceError;

            if (reduceError != null)
            {
                IncreaseErrorRecoverLevel();

                if (reduceError.ErrorPosition == null)
                {
                    reduceError.ErrorPosition = lookahead.Value.Span;
                }

                AddError(reduceError);
            }

#if HISTORY
            var to = m_topStack.StateIndex;
            History.Add(String.Format("R{0}:{1}", from, to));
#endif
        }

        /// <summary>
        /// Restore the stack to the state after last shift action, for error recovery
        /// </summary>
        public void RestoreToLastShift()
        {
            m_topStack = m_lastShiftStack;
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

        public static bool ShareSameParent(ParserHead h1, ParserHead h2)
        {
            var prev1 = h1.m_topStack.PrevNode;
            var prev2 = h2.m_topStack.PrevNode;
            if (prev1 != null && prev2 != null && Object.ReferenceEquals(prev1, prev2))
            {
                return true;
            }

            return false;
        }

        public IProduction PanicRecover(TransitionTable transitions, SourceSpan lastLocation, bool isEos)
        {
            while(true)
            {
                int currentStateIndex = m_topStack.StateIndex;
                for (int i = 0; i < transitions.ProductionCount; i++)
                {
                    int gotoState = transitions.GetGoto(currentStateIndex, i);

                    if (gotoState > 0)
                    {
                        var recoverNT = transitions.NonTerminals[i];

                        if (isEos)
                        {
                            //the recoverNT must have EOS in its follow, otherwise continue
                            var follow = (recoverNT as ProductionBase).Info.Follow;

                            if (!follow.Contains(EndOfStream.Instance))
                            {
                                continue;
                            }
                        }

                        m_topStack = m_topStack.PrevNode;

                        var newNode = new StackNode(gotoState, m_topStack, null);
                        IncreaseErrorRecoverLevel();
                        AddError(new ErrorRecord(null, lastLocation));
                        m_topStack = newNode;

                        return recoverNT;
                    }
                }

                if (m_topStack.PrevNode == null)
                {
                    throw new ParsingFailureException("There's no way to recover from parser error");
                }

                m_topStack = m_topStack.PrevNode;
                m_lastShiftStack = m_topStack;
            }
        }
    }
}
