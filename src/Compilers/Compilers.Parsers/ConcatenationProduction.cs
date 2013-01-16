using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class ConcatenationProduction<T1, T2, TR> : Production<TR>
    {
        public Production<T1> ProductionLeft { get; private set; }
        public Func<T1, Production<T2>> ProductionRightSelector { get; private set; }
        public Func<T1, T2, TR> Selector { get; private set; }

        public ConcatenationProduction(Production<T1> productionLeft, Func<T1, Production<T2>> productionRightSelector, Func<T1, T2, TR> selector)
        {
            CodeContract.RequiresArgumentNotNull(productionLeft, "productionLeft");
            CodeContract.RequiresArgumentNotNull(productionRightSelector, "productionRightSelector");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            ProductionLeft = productionLeft;
            ProductionRightSelector = productionRightSelector;
            Selector = selector;
        }

        protected internal override void Accept<TResult>(IProductionVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
