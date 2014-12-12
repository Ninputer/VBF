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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VBF.Compilers.Scanners.Generator
{
    [DebuggerDisplay("State #{Index}")]
    public class NFAState
    {
        private List<NFAEdge> m_outEdges;
        private ReadOnlyCollection<NFAEdge> m_readonlyOutEdges;

        internal NFAState()
        {
            m_outEdges = new List<NFAEdge>();
            m_readonlyOutEdges = new ReadOnlyCollection<NFAEdge>(m_outEdges);

            //default value -1 means no token is bound with this state
            TokenIndex = -1;
        }

        public int Index { get; internal set; }
        internal int TokenIndex { get; set; }

        public ReadOnlyCollection<NFAEdge> OutEdges
        {
            get
            {
                return m_readonlyOutEdges;
            }
        }

        internal void AddEmptyEdgeTo(NFAState targetState)
        {
            CodeContract.RequiresArgumentNotNull(targetState, "targetState");

            m_outEdges.Add(new NFAEdge(targetState));
        }

        internal void AddEdge(NFAEdge edge)
        {
            CodeContract.RequiresArgumentNotNull(edge, "edge");

            m_outEdges.Add(edge);
        }


    }
}
