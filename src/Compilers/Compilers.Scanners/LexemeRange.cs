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

using System.Collections;
using System.Collections.Generic;

namespace VBF.Compilers.Scanners
{
    public class LexemeRange : IReadOnlyList<Lexeme>
    {
        private readonly int m_length;
        private readonly IReadOnlyList<Lexeme> m_lexemeList;
        private readonly int m_startIndex;

        public LexemeRange(IReadOnlyList<Lexeme> lexemeList, int startIndex, int length)
        {
            CodeContract.RequiresArgumentNotNull(lexemeList, "lexemeList");
            CodeContract.RequiresArgumentInRange(startIndex >= 0 && startIndex <= lexemeList.Count,
                "startIndex", "startIndex must be greater or equal to 0 and less than the count of lexemeList");
            CodeContract.RequiresArgumentInRange(length >= 0 && startIndex + length <= lexemeList.Count,
                "length", "lengh is invalid");

            m_lexemeList = lexemeList;
            m_startIndex = startIndex;
            m_length = length;
        }


        public Lexeme this[int index]
        {
            get
            {
                CodeContract.RequiresArgumentInRange(index >= 0 && index < m_length, 
                    "index", "index must be greater than or equal to 0 and less than the length of the range");
                return m_lexemeList[index + m_startIndex];
            }
        }

        public int Count
        {
            get { return m_length; }
        }

        public IEnumerator<Lexeme> GetEnumerator()
        {
            for (int i = m_startIndex; i < m_startIndex + m_length; i++)
            {
                yield return m_lexemeList[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
