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

namespace VBF.Compilers.Parsers.Combinators
{
    public class AlternationParser<T> : Parser<T>
    {
        public AlternationParser(Parser<T> parser1, Parser<T> parser2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            Parser1 = parser1;
            Parser2 = parser2;
        }

        public Parser<T> Parser1 { get; private set; }
        public Parser<T> Parser2 { get; private set; }

        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) =>
            {
                var s1 = scanner;
                var s2 = scanner.Fork();

                return context.ChooseBest(Parser1.BuildParser(future)(s1, context), Parser2.BuildParser(future)(s2, context));
            };
        }

    }
}
