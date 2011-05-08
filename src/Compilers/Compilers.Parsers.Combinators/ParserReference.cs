using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ParserReference<T> : Parser<T>
    {
        public Parser<T> Reference { get; set; }

        public override Func<ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return Reference.Run(future);
        }
    }
}
