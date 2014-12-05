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
using System.Diagnostics;

namespace VBF.Compilers.Scanners.Generator
{
    public class CompactCharSetManager
    {
        private readonly ushort m_maxIndex;
        private readonly ushort m_minIndex = 1;
        private ushort[] m_compactCharTable;

        public CompactCharSetManager(ushort[] compactCharTable, ushort maxIndex)
        {
            m_compactCharTable = compactCharTable;
            Debug.Assert(maxIndex >= m_minIndex);
            m_maxIndex = maxIndex;
        }

        public int MaxClassIndex
        {
            get
            {
                return m_maxIndex;
            }
        }

        public int MinClassIndex
        {
            get
            {
                return m_minIndex;
            }
        }

        public int GetCompactClass(char c)
        {
            ushort compactClass = m_compactCharTable[(int)c];

            //0 is an invalid compact class
            Debug.Assert(compactClass >= m_minIndex);
            return (int)compactClass;
        }

        public bool HasCompactClass(char c)
        {
            return m_compactCharTable[c] >= m_minIndex;
        }

        public HashSet<char>[] CreateCompactCharMapTable()
        {
            HashSet<char>[] result = new HashSet<char>[m_maxIndex + 1];
            for (int i = 0; i <= m_maxIndex; i++)
            {
                result[i] = new HashSet<char>();
            }

            for (int i = Char.MinValue; i <= Char.MaxValue; i++)
            {
                int index = m_compactCharTable[i];

                result[index].Add((char)i);
            }

            return result;
        }
    }
}
