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

namespace VBF.Compilers.Parsers.Generator
{
    public struct LR0Edge : IEquatable<LR0Edge>
    {
        private int m_sourceStateIndex;
        private int m_symbolIndex;
        private int m_targetStateIndex;

        internal LR0Edge(int sourceStateIndex, int symbolIndex, int targetStateIndex)
        {
            m_sourceStateIndex = sourceStateIndex;
            m_symbolIndex = symbolIndex;
            m_targetStateIndex = targetStateIndex;
        }

        public int SymbolIndex
        {
            get
            {
                return m_symbolIndex;
            }
        }

        public int SourceStateIndex
        {
            get
            {
                return m_sourceStateIndex;
            }
        }

        public int TargetStateIndex
        {
            get
            {
                return m_targetStateIndex;
            }
        }

        public bool Equals(LR0Edge other)
        {
            return m_symbolIndex == other.m_symbolIndex && 
                   m_targetStateIndex == other.m_targetStateIndex &&
                   m_sourceStateIndex == other.m_sourceStateIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            LR0Edge other = (LR0Edge)obj;

            return Equals(other); 
        }

        public override int GetHashCode()
        {
            return (m_sourceStateIndex << 24) ^ (m_symbolIndex << 12) ^ m_targetStateIndex;
        }
    }
}
