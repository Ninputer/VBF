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
using System.Globalization;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// Represents a regular expression accepts a literal character
    /// </summary>
    public class SymbolExpression : RegularExpression
    {
        public SymbolExpression(char symbol)
            : base(RegularExpressionType.Symbol)
        {
            Symbol = symbol;
        }

        public new char Symbol { get; private set; }

        public override string ToString()
        {
            return Symbol.ToString(CultureInfo.InvariantCulture);
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return new Func<HashSet<char>>[0];
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            var result = new HashSet<char> {Symbol};

            return result;
        }

        internal override T Accept<T>(RegularExpressionConverter<T> converter)
        {
            return converter.ConvertSymbol(this);
        }
    }
}
