using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;
using System.Diagnostics;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ParserRunner<T>
    {
        public ParserContext Context { get; private set; }
        private ParserFunc<T> m_runner;

        public ParserRunner(Parser<T> parser, ParserContext context)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(context, "context");

            m_runner = parser.BuildParser(FinalFuture);
            Debug.Assert(m_runner != null);
            Context = context;
        }

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
