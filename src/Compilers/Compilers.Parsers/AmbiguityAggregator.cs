using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    internal abstract class AmbiguityAggregator
    {
        public int ProductionIndex { get; private set; }

        protected AmbiguityAggregator(int productionIndex)
        {
            ProductionIndex = productionIndex;
        }

        public abstract object Aggregate(object result1, object result2);
    }

    internal class AmbiguityAggregator<T> : AmbiguityAggregator
    {
        private Func<T, T, T> m_aggregator;

        public override object Aggregate(object result1, object result2)
        {
            return m_aggregator((T)result1, (T)result2);
        }

        public AmbiguityAggregator(int productionIndex, Func<T, T, T> aggregator)
            : base(productionIndex)
        {
            m_aggregator = aggregator;
        }
    }
}
