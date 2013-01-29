using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    internal static class SetHelpers
    {
        internal static bool UnionCheck<T>(this ISet<T> set, IEnumerable<T> toUnion)
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
