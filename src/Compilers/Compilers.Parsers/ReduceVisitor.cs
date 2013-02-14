using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VBF.Compilers.Parsers.Generator;

namespace VBF.Compilers.Parsers
{

    internal class ReduceVisitor : IProductionVisitor<StackNode, Tuple<StackNode, ErrorRecord>>
    {
        private TransitionTable m_transitions;

        public ReduceVisitor(TransitionTable transitions)
        {
            m_transitions = transitions;
        }

        Tuple<StackNode, ErrorRecord> IProductionVisitor<StackNode, Tuple<StackNode, ErrorRecord>>.VisitTerminal(Terminal terminal, StackNode TopStack)
        {
            throw new NotSupportedException("No need to reduce terminals");
        }

        Tuple<StackNode, ErrorRecord> IProductionVisitor<StackNode, Tuple<StackNode, ErrorRecord>>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, StackNode TopStack)
        {
            ErrorRecord ReduceError = null;

            StackNode topStack = TopStack;
            StackNode poppedTopStack;

            var info = ((ProductionBase)mappingProduction).Info;

            //pop one
            poppedTopStack = topStack.PrevNode;

            //reduce
            var result = mappingProduction.Selector((TSource)topStack.ReducedValue);

            //validate result
            if (mappingProduction.ValidationRule != null)
            {
                if (!mappingProduction.ValidationRule(result))
                {
                    SourceSpan position = null;

                    if (mappingProduction.PositionGetter != null)
                    {
                        position = mappingProduction.PositionGetter(result);
                    }

                    //generates error
                    ReduceError = new ErrorRecord(mappingProduction.ValidationErrorId, position);
                }
            }

            //compute goto
            var gotoAction = m_transitions.GetGoto(poppedTopStack.StateIndex, info.NonTerminalIndex);          

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, poppedTopStack, result);

            return Tuple.Create(reduceNode, ReduceError);
        }

        Tuple<StackNode, ErrorRecord> IProductionVisitor<StackNode, Tuple<StackNode, ErrorRecord>>.VisitEndOfStream(EndOfStream endOfStream, StackNode TopStack)
        {
            throw new NotSupportedException("No need to reduce terminal EOS");
        }

        Tuple<StackNode, ErrorRecord> IProductionVisitor<StackNode, Tuple<StackNode, ErrorRecord>>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, StackNode TopStack)
        {
            var info = ((ProductionBase)emptyProduction).Info;

            //insert a new value onto stack
            var result = emptyProduction.Value;

            //compute goto
            var gotoAction = m_transitions.GetGoto(TopStack.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, TopStack, result);

            return Tuple.Create<StackNode, ErrorRecord>(reduceNode, null);
        }

        Tuple<StackNode, ErrorRecord> IProductionVisitor<StackNode, Tuple<StackNode, ErrorRecord>>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, StackNode TopStack)
        {
           
            var info = ((ProductionBase)alternationProduction).Info;            

            //compute goto
            var gotoAction = m_transitions.GetGoto(TopStack.PrevNode.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, TopStack.PrevNode, TopStack.ReducedValue);

            return Tuple.Create<StackNode, ErrorRecord>(reduceNode, null);
        }

        Tuple<StackNode, ErrorRecord> IProductionVisitor<StackNode, Tuple<StackNode, ErrorRecord>>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, StackNode TopStack)
        {
            StackNode topStack = TopStack;
            StackNode poppedTopStack;

            var info = ((ProductionBase)concatenationProduction).Info;

            //pop two
            var val2 = topStack.ReducedValue;

            topStack = topStack.PrevNode;

            var val1 = topStack.ReducedValue;

            poppedTopStack = topStack.PrevNode;

            //reduce
            var result = concatenationProduction.Selector((T1)val1, (T2)val2);

            //compute goto
            var gotoAction = m_transitions.GetGoto(poppedTopStack.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, poppedTopStack, result);

            return Tuple.Create<StackNode, ErrorRecord>(reduceNode, null);
        }
    }
}
