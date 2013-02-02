using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public abstract class ProductionBase : IProduction
    {
        internal virtual ProductionInfo Info { get; set; }

        public abstract void Accept(IProductionVisitor visitor);

        public virtual bool IsTerminal
        {
            get { return false; }
        }

        public virtual bool IsEos
        {
            get { return false; }
        }

        protected ProductionBase()
        {

        }

        public virtual string DebugName
        {
            get
            {
                return "P";
            }
        }
    }

    public abstract class ProductionBase<T> : ProductionBase
    {
        public static ProductionBase<T> operator |(ProductionBase<T> p1, ProductionBase<T> p2)
        {
            return new AlternationProduction<T>(p1, p2);
        }

        protected ProductionBase()
        {

        }
    }
}
