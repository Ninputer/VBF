using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public abstract class ProductionBase : IProduction
    {
        internal virtual ProductionInfo Info { get; set; }

        public abstract void Accept(IProductionVisitor visitor);
       
    }

    public abstract class ProductionBase<T> : ProductionBase
    {
        public static ProductionBase<T> operator |(ProductionBase<T> p1, ProductionBase<T> p2)
        {
            return new AlternationProduction<T>(p1, p2);
        }
    }
}
