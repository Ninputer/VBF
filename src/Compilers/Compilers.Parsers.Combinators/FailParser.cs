using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers.Combinators
{
    public class FailParser<T> : Parser<T>
    {
        public override Func<Scanners.ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) => context.FailResult<TFuture>();
        }
    }
}
