using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using VBF.Compilers.Parsers.Generator;

namespace VBF.Compilers.Parsers
{
    [System.Diagnostics.DebuggerDisplay("{ToString()}")]
    public abstract class ProductionBase : IProduction
    {
        internal virtual ProductionInfo Info { get; set; }

        public abstract TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument);

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
                    return Info.Index.ToString(CultureInfo.InvariantCulture);
                }
                return String.Empty;
            }
        }

        internal abstract AmbiguityAggregator CreateAggregator();

        public abstract bool AggregatesAmbiguities { get; }
    }

    public abstract class ProductionBase<T> : ProductionBase
    {
        public static ProductionBase<T> operator |(ProductionBase<T> p1, ProductionBase<T> p2)
        {
            return new AlternationProduction<T>(p1, p2);
        }

        public virtual Func<T, T, T> AmbiguityAggregator { get; set; }

        public sealed override bool AggregatesAmbiguities
        {
            get { return AmbiguityAggregator != null; }
        }

        internal sealed override AmbiguityAggregator CreateAggregator()
        {
            return new AmbiguityAggregator<T>(Info.NonTerminalIndex, AmbiguityAggregator);
        }

        protected ProductionBase()
        {

        }
    }
}
