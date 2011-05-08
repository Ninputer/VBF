using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class SucceedParser<T> : Parser<T>
    {
        public T Value { get; private set; }

        public SucceedParser(T value)
        {
            Value = value;
        }
        public override Func<ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) => future(Value)(scanner, context);
        }
    }
}
