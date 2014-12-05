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

namespace VBF.Compilers.Scanners
{
    public class CharSetExpressionBuilder
    {
        private List<Action<RegularExpression>> m_expFutureList;
        private List<Func<char, bool>> m_predicateList;

        public CharSetExpressionBuilder()
        {
            m_expFutureList = new List<Action<RegularExpression>>();
            m_predicateList = new List<Func<char, bool>>();
        }

        public void DefineCharSet(Func<char, bool> charSetPredicate, Action<RegularExpression> regexFuture)
        {
            CodeContract.RequiresArgumentNotNull(charSetPredicate, "charSetPredicate");
            CodeContract.RequiresArgumentNotNull(regexFuture, "regexFuture");

            m_predicateList.Add(charSetPredicate);
            m_expFutureList.Add(regexFuture);
        }

        public void Build()
        {
            Debug.Assert(m_predicateList.Count == m_expFutureList.Count);

            if (m_predicateList.Count == 0)
            {
                return;
            }

            List<char>[] charSetList = new List<char>[m_predicateList.Count];

            for (int j = 0; j < m_predicateList.Count; j++)
            {
                charSetList[j] = new List<char>();
            }

            for (int i = Char.MinValue; i < Char.MaxValue; i++)
            {
                char c = (char)i;
                for (int j = 0; j < m_predicateList.Count; j++)
                {
                    if (m_predicateList[j](c))
                    {
                        charSetList[j].Add(c);
                    }
                }
            }

            for (int j = 0; j < m_predicateList.Count; j++)
            {
                RegularExpression charSetExp = new AlternationCharSetExpression(charSetList[j]);
                m_expFutureList[j](charSetExp);
            }
        }
    }
}
