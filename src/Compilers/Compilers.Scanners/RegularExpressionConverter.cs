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

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// Used to convert a regular expression to a value. One must inherit this class to implement custom logic.
    /// </summary>
    /// <typeparam name="T">The target type that the converter converts a regular expression to</typeparam>
    public abstract class RegularExpressionConverter<T>
    {
        protected RegularExpressionConverter() { }

        public T Convert(RegularExpression expression)
        {
            if (expression == null)
            {
                return default(T);
            }

            return expression.Accept(this);
        }

        public abstract T ConvertAlternation(AlternationExpression exp);

        public abstract T ConvertSymbol(SymbolExpression exp);

        public abstract T ConvertEmpty(EmptyExpression exp);

        public abstract T ConvertConcatenation(ConcatenationExpression exp);

        public abstract T ConvertAlternationCharSet(AlternationCharSetExpression exp);

        public abstract T ConvertStringLiteral(StringLiteralExpression exp);

        public abstract T ConvertKleeneStar(KleeneStarExpression exp);
    }
}
