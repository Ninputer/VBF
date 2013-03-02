using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.DataStructures
{
    public class DisjointSets<T>
    {
        private Dictionary<T, T> m_setsStorage;

        public DisjointSets(IEnumerable<T> elements)
        {
            CodeContract.RequiresArgumentNotNull(elements, "elements");

            IReadOnlyCollection<T> iroc = elements as IReadOnlyCollection<T>;

            if (iroc != null)
            {
                m_setsStorage = new Dictionary<T, T>(iroc.Count);
            }
            else
            {
                m_setsStorage = new Dictionary<T, T>();
            }

            foreach (var item in elements)
            {
                if (item != null)
                {
                    m_setsStorage[item] = item;
                }
            }
        }

        private T Find(T v)
        {
            var baseNode = m_setsStorage[v];
            if (baseNode.Equals(v))
                return v;

            return m_setsStorage[v] = Find(baseNode);
        }

        public bool AreInTheSameSet(T v1, T v2)
        {
            CodeContract.RequiresArgumentNotNull(v1, "v1");
            CodeContract.RequiresArgumentNotNull(v2, "v2");

            return Object.Equals(Find(v1), Find(v2));
        }

        public void Union(T v1, T v2)
        {
            CodeContract.RequiresArgumentNotNull(v1, "v1");
            CodeContract.RequiresArgumentNotNull(v2, "v2");

            m_setsStorage[Find(v1)] = Find(v2);
        }
    }
}
