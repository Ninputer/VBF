using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class ProductionInfoManager
    {
        private IProduction[] productions;

        public IProduction RootProduction { get; private set; }

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

        public ProductionInfoManager(IProduction root)
        {
            CodeContract.RequiresArgumentNotNull(root, "root");

            var aggregator = new ProductionAggregationVisitor();
            root.Accept(aggregator);

            productions = aggregator.Productions.ToArray();
            RootProduction = root;

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
