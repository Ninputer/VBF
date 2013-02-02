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

        public MappingProduction(ProductionBase<TSource> sourceProduction, Func<TSource, TReturn> selector)
        {
            CodeContract.RequiresArgumentNotNull(sourceProduction, "sourceProduction");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            SourceProduction = sourceProduction;
            Selector = selector;
        }

        public override void Accept(IProductionVisitor visitor)
        {
            visitor.VisitMapping(this);
        }

        public override string DebugName
        {
            get
            {
                return "M" + Info.Index;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} ::= {1}", DebugName, SourceProduction.DebugName);
        }
    }
}
