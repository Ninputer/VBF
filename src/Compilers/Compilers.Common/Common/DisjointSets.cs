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

using System;
using System.Collections.Generic;

namespace VBF.Compilers.Common
{
    public class DisjointSets<T>
    {
        private Dictionary<T, int> m_ranks;
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

            return Equals(Find(v1), Find(v2));
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
