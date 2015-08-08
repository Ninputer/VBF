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
using System.Runtime.CompilerServices;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// The base class of regular expressions. Provides methods to build a regular expression
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public abstract class RegularExpression
    {
        protected RegularExpression(RegularExpressionType expType)
        {
            ExpressionType = expType;
        }

        public RegularExpressionType ExpressionType { get; private set; }

        internal abstract Func<HashSet<char>>[] GetCompactableCharSets();
        internal abstract HashSet<char> GetUncompactableCharSet();
        internal abstract T Accept<T>(RegularExpressionConverter<T> converter);

        //basic operations

        public static RegularExpression Symbol(char c)
        {
            return new SymbolExpression(c);
        }

        public RegularExpression Many()
        {
            if (ExpressionType == RegularExpressionType.KleeneStar)
            {
                return this;
            }

            return new KleeneStarExpression(this);
        }

        public RegularExpression Concat(RegularExpression follow)
        {
            return new ConcatenationExpression(this, follow);
        }

        public RegularExpression Union(RegularExpression other)
        {
            if (this.Equals(other))
            {
                return this;
            }

            return new AlternationExpression(this, other);
        }

        public static RegularExpression Literal(string literal)
        {
            return new StringLiteralExpression(literal);
        }

        public static RegularExpression CharSet(IEnumerable<char> charSet)
        {
            return new AlternationCharSetExpression(charSet);
        }

        public static RegularExpression CharSet(params char[] charSet)
        {
            return new AlternationCharSetExpression(charSet);
        }

        public static RegularExpression Empty()
        {
            return EmptyExpression.Instance;
        }

        //extended operations

        public RegularExpression Many1()
        {
            return this.Concat(this.Many());
        }

        public RegularExpression Optional()
        {
            return this.Union(Empty());
        }

        public static RegularExpression Range(char min, char max)
        {
            CodeContract.Requires(min <= max, "max", "The lower bound must be less than or equal to upper bound");

            List<char> rangeCharSet = new List<char>();
            for (char c = min; c <= max; c++)
            {
                rangeCharSet.Add(c);
            }

            return new AlternationCharSetExpression(rangeCharSet);
        }

        public static RegularExpression CharsOf(Func<char, bool> charPredicate)
        {
            CodeContract.RequiresArgumentNotNull(charPredicate, "charPredicate");

            List<char> charSet = new List<char>();
            for (int i = Char.MinValue; i <= Char.MaxValue; i++)
            {
                if (charPredicate((char)i)) charSet.Add((char)i);
            }

            return new AlternationCharSetExpression(charSet);
        }

        public RegularExpression Repeat(int number)
        {
            if (number <= 0)
            {
                return Empty();
            }

            RegularExpression result = this;

            for (int i = 1; i < number; i++)
            {
                result = result.Concat(this);
            }

            return result;
        }

        //operator overloading

        public static RegularExpression operator|(RegularExpression left, RegularExpression right)
        {
            return new AlternationExpression(left, right);
        }

        [SpecialName]
        public static RegularExpression op_RightShift(RegularExpression left, RegularExpression right)
        {
            return new ConcatenationExpression(left, right);
        }

        [SpecialName]
        public static RegularExpression op_Concatenate(RegularExpression left, RegularExpression right)
        {
            return new ConcatenationExpression(left, right);
        }
    }
}
