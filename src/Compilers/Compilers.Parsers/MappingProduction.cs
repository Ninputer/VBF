using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class MappingProduction<TSource, TReturn> : ProductionBase<TReturn>
    {
        public ProductionBase<TSource> SourceProduction { get; private set; }
        public Func<TSource, TReturn> Selector { get; private set; }
        public Func<TReturn, bool> ValidationRule { get; private set; }
        public int? ValidationErrorId { get; private set; }
        public Func<TReturn, SourceSpan> PositionGetter { get; private set; }

        public MappingProduction(ProductionBase<TSource> sourceProduction, Func<TSource, TReturn> selector, Func<TReturn, bool> validationRule, int? errorId, Func<TReturn, SourceSpan> positionGetter)
        {
            CodeContract.RequiresArgumentNotNull(sourceProduction, "sourceProduction");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            SourceProduction = sourceProduction;
            Selector = selector;
            ValidationRule = validationRule;
            ValidationErrorId = errorId;
            PositionGetter = positionGetter;
        }

        public MappingProduction(ProductionBase<TSource> sourceProduction, Func<TSource, TReturn> selector) : this(sourceProduction, selector, null, null, null) { }

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument)
        {
            return visitor.VisitMapping(this, argument);
        }

        public override string DebugName
        {
            get
            {
                return "M" + DebugNameSuffix;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} ::= {1}", DebugName, SourceProduction.DebugName);
        }
    }
}
