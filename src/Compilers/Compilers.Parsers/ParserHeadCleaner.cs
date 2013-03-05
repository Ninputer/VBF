using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    /// <summary>
    /// aggregates ambiguity heads
    /// </summary>
    internal class ParserHeadCleaner
    {
        private List<ParserHead> m_aggregatingHeads;

        public ParserHeadCleaner()
        {
            m_aggregatingHeads = new List<ParserHead>();
        }

        public void CleanHeads(IList<ParserHead> sourceHeads, IList<ParserHead> targetHeads)
        {
            int minErrorLevel = sourceHeads[0].ErrorRecoverLevel;
            int minErrorCount = sourceHeads[0].Errors != null ? sourceHeads[0].Errors.Count : 0;

            for (int i = 0; i < sourceHeads.Count; i++)
            {
                var head = sourceHeads[i];
                var errorLevel = head.ErrorRecoverLevel;
                if (errorLevel < minErrorLevel)
                {
                    minErrorLevel = errorLevel;
                }

                var errorCount = head.Errors != null ? head.Errors.Count : 0;
                if (errorCount < minErrorCount)
                {
                    minErrorCount = errorCount;
                }
            }

            foreach (var head in sourceHeads)
            {
                if (head.ErrorRecoverLevel > minErrorLevel)
                {
                    //discard heads with higher error level
                    continue;
                }

                var errorCount = head.Errors != null ? head.Errors.Count : 0;
                if (errorCount > minErrorCount)
                {
                    //discard heads with more errors
                    continue;
                }

                if (head.AmbiguityAggregator == null)
                {
                    targetHeads.Add(head);
                    continue;
                }

                //aggregate ambiguities
                bool isAggregated = false;
                foreach (var ambhead in m_aggregatingHeads)
                {
                    if (ambhead.TopStackStateIndex == head.TopStackStateIndex &&
                        ambhead.AmbiguityAggregator.ProductionIndex == head.AmbiguityAggregator.ProductionIndex &&
                        ParserHead.ShareSameParent(ambhead, head))
                    {
                        var aggregator = ambhead.AmbiguityAggregator;

                        //update aggregate value
                        ambhead.TopStackValue = aggregator.Aggregate(ambhead.TopStackValue, head.TopStackValue);

                        //discard he aggregated head
                        isAggregated = true;

                        break;
                    }
                }

                if (!isAggregated)
                {
                    m_aggregatingHeads.Add(head);
                    targetHeads.Add(head);
                }
            }

            foreach (var aggHead in m_aggregatingHeads)
            {
                aggHead.AmbiguityAggregator = null;
            }

            m_aggregatingHeads.Clear();
        }
    }
}
