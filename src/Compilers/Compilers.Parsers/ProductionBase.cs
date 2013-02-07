using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Parsers.Generator;

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

        protected string DebugNameSuffix
        {
            get
            {
                if (Info != null)
                {
                    return Info.Index.ToString();
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        /// <summary>
        /// Larger number means higher priority
        /// </summary>
        public virtual int Priority { get; set; }
        
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
