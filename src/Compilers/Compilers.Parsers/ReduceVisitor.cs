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
using VBF.Compilers.Parsers.Generator;

namespace VBF.Compilers.Parsers
{
    internal struct ReduceResult
    {
        public StackNode NewTopStack;
        public ErrorRecord ReduceError;

        public ReduceResult(StackNode newTopStack, ErrorRecord reduceError = null)
        {
            NewTopStack = newTopStack;
            ReduceError = reduceError;
        }
    }

    internal class ReduceVisitor : IProductionVisitor<StackNode, ReduceResult>
    {
        private TransitionTable m_transitions;

        public ReduceVisitor(TransitionTable transitions)
        {
            m_transitions = transitions;
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitTerminal(Terminal terminal, StackNode topStack)
        {
            throw new NotSupportedException("No need to reduce terminals");
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, StackNode topStack)
        {
            ErrorRecord reduceError = null;

            var info = ((ProductionBase)mappingProduction).Info;

            //pop one
            StackNode poppedTopStack = topStack.PrevNode;

            //reduce
            var result = mappingProduction.Selector((TSource)topStack.ReducedValue);

            //validate result
            if (mappingProduction.ValidationRule != null)
            {
                bool validation = false;

                try
                {
                    validation = mappingProduction.ValidationRule(result);
                }
                catch (Exception)
                {
                    validation = false;
                }
                
                if (!validation)
                {
                    SourceSpan position = null;

                    if (mappingProduction.PositionGetter != null)
                    {
                        try
                        {
                            position = mappingProduction.PositionGetter(result);
                        }
                        catch (Exception)
                        {
                            position = null;
                        }
                    }

                    //generates error
                    reduceError = new ErrorRecord(mappingProduction.ValidationErrorId, position);
                }
            }

            //compute goto
            var gotoAction = m_transitions.GetGoto(poppedTopStack.StateIndex, info.NonTerminalIndex);          

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, poppedTopStack, result);

            return new ReduceResult(reduceNode, reduceError);
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitEndOfStream(EndOfStream endOfStream, StackNode topStack)
        {
            throw new NotSupportedException("No need to reduce terminal EOS");
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, StackNode topStack)
        {
            var info = ((ProductionBase)emptyProduction).Info;

            //insert a new value onto stack
            var result = emptyProduction.Value;

            //compute goto
            var gotoAction = m_transitions.GetGoto(topStack.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, topStack, result);

            return new ReduceResult(reduceNode);
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, StackNode topStack)
        {
           
            var info = ((ProductionBase)alternationProduction).Info;            

            //compute goto
            var gotoAction = m_transitions.GetGoto(topStack.PrevNode.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, topStack.PrevNode, topStack.ReducedValue);

            return new ReduceResult(reduceNode);
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, StackNode topStack)
        {

            var info = ((ProductionBase)concatenationProduction).Info;

            //pop two
            var val2 = topStack.ReducedValue;

            topStack = topStack.PrevNode;

            var val1 = topStack.ReducedValue;

            StackNode poppedTopStack = topStack.PrevNode;

            //reduce
            var result = concatenationProduction.Selector((T1)val1, (T2)val2);

            //compute goto
            var gotoAction = m_transitions.GetGoto(poppedTopStack.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, poppedTopStack, result);

            return new ReduceResult(reduceNode);
        }
    }
}
