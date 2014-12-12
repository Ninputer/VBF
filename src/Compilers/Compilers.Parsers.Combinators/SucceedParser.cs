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
    public class SucceedParser<T> : Parser<T>
    {
        public SucceedParser(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) => future(Value)(scanner, context);
        }
    }
}
