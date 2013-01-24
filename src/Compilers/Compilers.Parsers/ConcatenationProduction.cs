using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class ConcatenationProduction<T1, T2, TR> : ProductionBase<TR>
    {
        public ProductionBase<T1> ProductionLeft { get; private set; }
        public ProductionBase<T2> ProductionRight { get; private set; }
        public Func<T1, T2, TR> Selector { get; private set; }

        public ConcatenationProduction(ProductionBase<T1> productionLeft, Func<T1, ProductionBase<T2>> productionRightSelector, Func<T1, T2, TR> selector)
        {
            CodeContract.RequiresArgumentNotNull(productionLeft, "productionLeft");
            CodeContract.RequiresArgumentNotNull(productionRightSelector, "productionRightSelector");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            ProductionLeft = productionLeft;
            ProductionRight = productionRightSelector(default(T1));
            Selector = selector;
        }

        public override void Accept(IProductionVisitor visitor)
        {
            visitor.VisitConcatenation(this);
        }
    }
}
