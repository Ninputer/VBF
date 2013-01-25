using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class ProductionInfoService
    {
        private IProduction[] productions;

        public IReadOnlyList<IProduction> Productions
        {
            get
            {
                return productions;
            }
        }

        public ProductionInfo GetInfo(IProduction production)
        {
            return (production as ProductionBase).Info;
        }

        public ProductionInfoService(IProduction root)
        {
            CodeContract.RequiresArgumentNotNull(root, "root");

            var aggregator = new ProductionAggregationVisitor();
            root.Accept(aggregator);

            productions = aggregator.Productions.ToArray();

            var ffVisitor = new FirstFollowVisitor();

            do
            {
                ffVisitor.IsChanged = false;

                foreach (var p in productions)
                {
                    p.Accept(ffVisitor);
                }

            } while (ffVisitor.IsChanged);
        }
    }
}
