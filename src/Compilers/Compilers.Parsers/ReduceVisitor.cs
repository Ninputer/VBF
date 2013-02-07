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

    internal class ReduceVisitor : IProductionVisitor
    {
        private TransitionTable m_transitions;
        internal StackNode TopStack;
        internal StackNode NewTopStack;

        internal ErrorRecord ReduceError;

        public ReduceVisitor(TransitionTable transitions)
        {
            m_transitions = transitions;
        }

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            throw new NotSupportedException("No need to reduce terminals");
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
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

            //TODO: optimize goto table if no conflicts
            Debug.Assert(gotoAction.GetNext() == null, "goto action is not unique");

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction.Value, poppedTopStack, result);

            NewTopStack = reduceNode;
        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
            throw new NotSupportedException("No need to reduce terminal EOS");
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            var info = ((ProductionBase)emptyProduction).Info;

            //insert a new value onto stack
            var result = emptyProduction.Value;

            //compute goto
            var gotoAction = m_transitions.GetGoto(TopStack.StateIndex, info.NonTerminalIndex);

            //TODO: optimize goto table if no conflicts
            Debug.Assert(gotoAction.GetNext() == null, "goto action is not unique");

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction.Value, TopStack, result);

            NewTopStack = reduceNode;
        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {
           
            var info = ((ProductionBase)alternationProduction).Info;            

            //compute goto
            var gotoAction = m_transitions.GetGoto(TopStack.PrevNode.StateIndex, info.NonTerminalIndex);

            //TODO: optimize goto table if no conflicts
            Debug.Assert(gotoAction.GetNext() == null, "goto action is not unique");

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction.Value, TopStack.PrevNode, TopStack.ReducedValue);

            NewTopStack = reduceNode;
        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
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

            //TODO: optimize goto table if no conflicts
            Debug.Assert(gotoAction.GetNext() == null, "goto action is not unique");

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction.Value, poppedTopStack, result);

            NewTopStack = reduceNode;
        }
    }
}
