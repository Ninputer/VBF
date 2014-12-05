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

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// Represents a regular expression accepts an empty set
    /// </summary>
    public sealed class EmptyExpression : RegularExpression
    {
        public static readonly EmptyExpression Instance = new EmptyExpression();
        private EmptyExpression() : base(RegularExpressionType.Empty) { }

        public override string ToString()
        {
            return "ε";
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return new Func<HashSet<char>>[0];
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            return new HashSet<char>();
        }

        internal override T Accept<T>(RegularExpressionConverter<T> converter)
        {
            return converter.ConvertEmpty(this);
        }
    }
}
