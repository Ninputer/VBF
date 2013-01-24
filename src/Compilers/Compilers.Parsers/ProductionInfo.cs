using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    internal class ProductionInfo
    {
        public ISet<IProduction> First { get; private set; }
        public ISet<IProduction> Follow { get; private set; }
        public bool IsNullable { get; set; }

        public ProductionInfo()
        {
            First = new HashSet<IProduction>();
            Follow = new HashSet<IProduction>();
            IsNullable = false;
        }

        public static bool UnionSet<T>(ISet<T> set, IEnumerable<T> toUnion)
        {
            bool changed = false;

            foreach (var item in toUnion)
            {
                changed = set.Add(item) || changed;
            }

            return changed;
        }
    }
    
}
