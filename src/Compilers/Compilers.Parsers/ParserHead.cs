// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private int m_errorRecoverLevel;
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
            if (other.m_errors == null || m_errors == null)
            {
                return false;
            }
            HashSet<ErrorRecord> myErrors = new HashSet<ErrorRecord>(m_errors);
            HashSet<ErrorRecord> otherErrors = new HashSet<ErrorRecord>(other.m_errors);

            return myErrors.SetEquals(otherErrors);
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
                AmbiguityAggregator = ((ProductionBase)production).CreateAggregator();
            }

            var reduceResult = production.Accept(reducer, m_topStack);

            m_topStack = reduceResult.NewTopStack;
            var reduceError = reduceResult.ReduceError;

            if (reduceError != null)
            {
                IncreaseErrorRecoverLevel();

                if (reduceError.ErrorPosition == null)
                {
                    reduceError = new ErrorRecord(reduceError.ErrorId, lookahead.Value.Span);
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
            if (prev1 != null && prev2 != null && ReferenceEquals(prev1, prev2))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<Tuple<ParserHead,IProduction>> PanicRecover(TransitionTable transitions, SourceSpan lastLocation, bool isEos)
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
                            var follow = ((ProductionBase)recoverNT).Info.Follow;

                            if (!follow.Contains(EndOfStream.Instance))
                            {
                                continue;
                            }
                        }

                        //m_topStack = m_topStack.PrevNode;

                        var newNode = new StackNode(gotoState, m_topStack, ((ProductionBase)recoverNT).GetDefaultResult());

                        var newHead = Clone();
                        newHead.m_topStack = newNode;

                        newHead.IncreaseErrorRecoverLevel();
                        newHead.AddError(new ErrorRecord(null, lastLocation));
                        

                        yield return Tuple.Create(newHead, recoverNT);
                    }
                }

                if (m_topStack.PrevNode == null)
                {
                    yield break;
                    //throw new ParsingFailureException("There's no way to recover from parser error");
                }

                m_topStack = m_topStack.PrevNode;
                m_lastShiftStack = m_topStack;
            }
        }
    }
}
