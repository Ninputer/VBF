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

using System.Diagnostics;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ParserRunner<T>
    {
        private ParserFunc<T> m_runner;

        public ParserRunner(Parser<T> parser, ParserContext context)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(context, "context");

            m_runner = parser.BuildParser(FinalFuture);
            Debug.Assert(m_runner != null);
            Context = context;
        }

        public ParserContext Context { get; private set; }

        public T Run(ForkableScanner scanner)
        {
            var result = m_runner(scanner, Context);
            return result.GetResult(Context);
        }

        private ParserFunc<T> FinalFuture(T value)
        {
            return (scanner, context) => context.StopResult(value);
        }
    }
}
