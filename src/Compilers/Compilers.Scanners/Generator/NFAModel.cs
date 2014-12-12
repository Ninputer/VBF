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

namespace VBF.Compilers.Scanners.Generator
{
    public class NFAModel
    {
        private ReadOnlyCollection<NFAState> m_readonlyStates;
        private List<NFAState> m_states;

        internal NFAModel()
        {
            m_states = new List<NFAState>();
            m_readonlyStates = new ReadOnlyCollection<NFAState>(m_states);
        }

        public NFAState TailState { get; internal set; }
        public NFAEdge EntryEdge { get; internal set; }

        public ReadOnlyCollection<NFAState> States
        {
            get
            {
                return m_readonlyStates;
            }
        }

        internal void AddState(NFAState state)
        {
            m_states.Add(state);
            state.Index = m_states.Count - 1;
        }

        internal void AddStates(IEnumerable<NFAState> states)
        {
            foreach (var s in states)
            {
                AddState(s);
            }
        }
    }
}
