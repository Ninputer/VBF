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

using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public delegate Result<T> ParserFunc<T>(ForkableScanner scanner, ParserContext context);
    public delegate ParserFunc<TFuture> Future<in T, TFuture>(T value);

    public abstract class Parser<T>
    {
        public abstract ParserFunc<TFuture> BuildParser<TFuture>(Future<T, TFuture> future);

        public static Parser<T> operator |(Parser<T> p1, Parser<T> p2)
        {
            return new AlternationParser<T>(p1, p2);
        }

        public Parser<TResult> Convert<TResult>()
        {
            CodeContract.RequiresArgumentNotNull(this, "parser");

            return new MappingParser<T, TResult>(this, ConvertHelper<T, TResult>.Convert);
        }

        public Parser<TResult> TryCast<TResult>()
            where TResult : class
        {
            CodeContract.RequiresArgumentNotNull(this, "parser");

            return new MappingParser<T, TResult>(this, t => t as TResult);
        }
    }
}
