// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;

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
            //int minErrorCount = sourceHeads[0].Errors != null ? sourceHeads[0].Errors.Count : 0;

            for (int i = 0; i < sourceHeads.Count; i++)
            {
                var head = sourceHeads[i];
                var errorLevel = head.ErrorRecoverLevel;
                if (errorLevel < minErrorLevel)
                {
                    minErrorLevel = errorLevel;
                }

                //var errorCount = head.Errors != null ? head.Errors.Count : 0;
                //if (errorCount < minErrorCount)
                //{
                //    minErrorCount = errorCount;
                //}
            }

            foreach (var head in sourceHeads)
            {
                if (head.ErrorRecoverLevel > minErrorLevel)
                {
                    //discard heads with higher error level
                    continue;
                }

                //var errorCount = head.Errors != null ? head.Errors.Count : 0;
                //if (errorCount > minErrorCount)
                //{
                //    //discard heads with more errors
                //    continue;
                //}

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

                        //if the head have different errors, they are probably came from error recovery
                        //in this case, the aggregation just keep the first one and discard others
                        //this way won't increase the total amount of errors
                        if (ambhead.HasSameErrorsWith(head))
                        {
                            //update aggregate value
                            ambhead.TopStackValue = aggregator.Aggregate(ambhead.TopStackValue, head.TopStackValue);
                        }

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
