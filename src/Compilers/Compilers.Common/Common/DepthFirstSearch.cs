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
using System.Linq;

namespace VBF.Compilers.Common
{
    public delegate IEnumerable<T> ChildrenGetter<T>(T node);

    public class DepthFirstSearch<T>
    {
        private ChildrenGetter<T> m_childrenGetter;
        private int m_componentNumber;
        private Dictionary<T, int> m_nodeIndexLookup;
        private T[] m_nodes;
        private DFSInfo[] m_nodesInfo;
        private int m_postClock;
        private int m_preClock;
        private bool[] m_visited;

        public DepthFirstSearch(IEnumerable<T> nodes, ChildrenGetter<T> childrenGetter)
        {
            CodeContract.RequiresArgumentNotNull(nodes, "nodes");
            CodeContract.RequiresArgumentNotNull(childrenGetter, "childrenGetter");

            m_nodes = nodes.ToArray();
            m_childrenGetter = childrenGetter;

            m_visited = new bool[m_nodes.Length];
            m_nodesInfo = new DFSInfo[m_nodes.Length];
            m_nodeIndexLookup = new Dictionary<T, int>();

            for (int i = 0; i < m_nodes.Length; i++)
            {
                m_nodeIndexLookup[m_nodes[i]] = i;
            }
        }

        protected T[] Nodes
        {
            get
            {
                return m_nodes;
            }
        }

        protected DFSInfo[] NodesInfo
        {
            get
            {
                return m_nodesInfo;
            }
        }

        protected virtual void PreVisit(int nodeIndex)
        {
            m_nodesInfo[nodeIndex].ComponentNumber = m_componentNumber;
            m_nodesInfo[nodeIndex].PreIndex = m_preClock;
            ++m_preClock;
        }

        protected virtual void PostVisit(int nodeIndex)
        {
            m_nodesInfo[nodeIndex].PostIndex = m_postClock;
            ++m_postClock;
        }

        private void Explore(int nodeIndex)
        {
            m_visited[nodeIndex] = true;
            PreVisit(nodeIndex);

            var children = m_childrenGetter(m_nodes[nodeIndex]);
            foreach (var child in children)
            {
                var childIndex = m_nodeIndexLookup[child];
                if (!m_visited[childIndex])
                {
                    m_nodesInfo[childIndex].PrevNodeIndex = nodeIndex;
                    Explore(childIndex);
                }
            }

            PostVisit(nodeIndex);
        }

        public void Start()
        {
            m_visited.Initialize();
            m_preClock = 0;
            m_postClock = 0;

            for (int i = 0; i < m_nodes.Length; i++)
            {
                if (!m_visited[i])
                {
                    ++m_componentNumber;
                    Explore(i);
                }
            }
        }

        public int GetPreIndex(T node)
        {
            return m_nodesInfo[GetIndex(node)].PreIndex;
        }

        public int GetPostIndex(T node)
        {
            return m_nodesInfo[GetIndex(node)].PostIndex;
        }

        public T GetParentInDFSTree(T node)
        {
            return m_nodes[m_nodesInfo[GetIndex(node)].PrevNodeIndex];
        }

        private int GetIndex(T node)
        {
            return m_nodeIndexLookup[node];
        }

        protected struct DFSInfo
        {
            public int ComponentNumber;
            public int PostIndex;
            public int PreIndex;
            public int PrevNodeIndex;
        }
    }
}
