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
        private Dictionary<T, int> m_ranks;

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

            m_ranks = new Dictionary<T, int>();

            foreach (var item in elements)
            {
                if (item != null)
                {
                    m_setsStorage[item] = item;
                    m_ranks[item] = 0;
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

            var r1 = Find(v1);
            var r2 = Find(v2);

            var rank1 =  m_ranks[r1];
            var rank2 = m_ranks[r2];
            if (rank1 > rank2)
            {
                m_setsStorage[r2] = r1;
            }
            else
            {
                if (rank1 == rank2)
                {
                    m_ranks[r2] = rank2 + 1;
                }

                m_setsStorage[r1] = r2;
            }

        }
    }
}
