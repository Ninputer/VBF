using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Generator
{
    public class ProductionInfoManager
    {
        private IProduction[] m_productions;

        public IProduction RootProduction { get; private set; }

        public IReadOnlyList<IProduction> Productions
        {
            get
            {
                return m_productions;
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
            var productions = root.Accept(aggregator, new List<IProduction>());

            m_productions = productions.ToArray();
            RootProduction = root;

            var ffVisitor = new FirstFollowVisitor();

            bool isChanged;

            do
            {
                isChanged = false;

                foreach (var p in productions)
                {
                    isChanged = p.Accept(ffVisitor, isChanged);
                }

            } while (isChanged);
        }
    }
}
