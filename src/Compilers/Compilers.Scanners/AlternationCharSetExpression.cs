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
using System.Collections.ObjectModel;

namespace VBF.Compilers.Scanners
{
    public sealed class AlternationCharSetExpression : RegularExpression
    {
        private List<char> m_charSet;

        public AlternationCharSetExpression(IEnumerable<char> charset)
            : base(RegularExpressionType.AlternationCharSet)
        {
            m_charSet = new List<char>(charset);
        }

        public new ReadOnlyCollection<char> CharSet
        {
            get
            {
                return m_charSet.AsReadOnly();
            }
        }

        public override string ToString()
        {
            if (m_charSet.Count == 0)
            {
                return String.Empty;
            }

            return '[' + new String(m_charSet.ToArray()) + ']';
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return new Func<HashSet<char>>[] { () => new HashSet<char>(m_charSet) };
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            return new HashSet<char>();
        }

        internal override T Accept<T>(RegularExpressionConverter<T> converter)
        {
            return converter.ConvertAlternationCharSet(this);
        }
    }
}
