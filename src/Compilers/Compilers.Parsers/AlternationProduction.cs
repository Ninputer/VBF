using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class AlternationProduction<T> : ProductionBase<T>
    {
        public ProductionBase<T> Production1 { get; private set; }
        public ProductionBase<T> Production2 { get; private set; }

        public AlternationProduction(ProductionBase<T> production1, ProductionBase<T> production2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            Production1 = production1;
            Production2 = production2;
        }

        public override void Accept(IProductionVisitor visitor)
        {
            visitor.VisitAlternation(this);
        }
    }
}
