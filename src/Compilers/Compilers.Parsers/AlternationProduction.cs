using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class AlternationProduction<T> : Production<T>
    {
        public Production<T> Production1 { get; private set; }
        public Production<T> Production2 { get; private set; }

        public AlternationProduction(Production<T> production1, Production<T> production2)
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
