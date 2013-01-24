using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public abstract class ProductionBase<T> : IProduction
    {
        internal virtual ProductionInfo Info { get; set; }

        public abstract void Accept(IProductionVisitor visitor);
       
    }
}
