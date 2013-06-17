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

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitTerminal(Terminal terminal, StackNode TopStack)
        {
            throw new NotSupportedException("No need to reduce terminals");
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, StackNode TopStack)
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
                    ReduceError = new ErrorRecord(mappingProduction.ValidationErrorId, position);
                }
            }

            //compute goto
            var gotoAction = m_transitions.GetGoto(poppedTopStack.StateIndex, info.NonTerminalIndex);          

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, poppedTopStack, result);

            return new ReduceResult(reduceNode, ReduceError);
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitEndOfStream(EndOfStream endOfStream, StackNode TopStack)
        {
            throw new NotSupportedException("No need to reduce terminal EOS");
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, StackNode TopStack)
        {
            var info = ((ProductionBase)emptyProduction).Info;

            //insert a new value onto stack
            var result = emptyProduction.Value;

            //compute goto
            var gotoAction = m_transitions.GetGoto(TopStack.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, TopStack, result);

            return new ReduceResult(reduceNode);
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, StackNode TopStack)
        {
           
            var info = ((ProductionBase)alternationProduction).Info;            

            //compute goto
            var gotoAction = m_transitions.GetGoto(TopStack.PrevNode.StateIndex, info.NonTerminalIndex);

            //perform goto
            StackNode reduceNode = new StackNode(gotoAction, TopStack.PrevNode, TopStack.ReducedValue);

            return new ReduceResult(reduceNode);
        }

        ReduceResult IProductionVisitor<StackNode, ReduceResult>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, StackNode TopStack)
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

            return new ReduceResult(reduceNode);
        }
    }
}
