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
    ///  Represents a concatenation of two regular expressions 
    /// </summary>
    public sealed class ConcatenationExpression : RegularExpression
    {
        public ConcatenationExpression(RegularExpression left, RegularExpression right)
            : base(RegularExpressionType.Concatenation)
        {
            CodeContract.RequiresArgumentNotNull(left, "left");
            CodeContract.RequiresArgumentNotNull(right, "right");

            Left = left;
            Right = right;
        }

        public RegularExpression Left { get; private set; }
        public RegularExpression Right { get; private set; }

        public override string ToString()
        {
            return Left.ToString() + Right.ToString();
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return Left.GetCompactableCharSets().Union(Right.GetCompactableCharSets()).ToArray();
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            var result = Left.GetUncompactableCharSet();
            result.UnionWith(Right.GetUncompactableCharSet());

            return result;
        }

        internal override T Accept<T>(RegularExpressionConverter<T> converter)
        {
            return converter.ConvertConcatenation(this);
        }
    }
}
