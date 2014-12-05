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
using System.Linq;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// Represents an alternation of two regular expressions 
    /// </summary>
    public sealed class AlternationExpression : RegularExpression
    {
        public AlternationExpression(RegularExpression expression1, RegularExpression expression2)
            : base(RegularExpressionType.Alternation)
        {
            CodeContract.RequiresArgumentNotNull(expression1, "expression1");
            CodeContract.RequiresArgumentNotNull(expression2, "expression2");

            Expression1 = expression1;
            Expression2 = expression2;
        }

        public RegularExpression Expression1 { get; private set; }
        public RegularExpression Expression2 { get; private set; }

        public override string ToString()
        {
            return '(' + Expression1.ToString() + '|' + Expression2.ToString() +')';
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return Expression1.GetCompactableCharSets().Union(Expression2.GetCompactableCharSets()).ToArray();
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            var result = Expression1.GetUncompactableCharSet();
            result.UnionWith(Expression2.GetUncompactableCharSet());

            return result;
        }

        internal override T Accept<T>(RegularExpressionConverter<T> converter)
        {
            return converter.ConvertAlternation(this);
        }
    }
}
