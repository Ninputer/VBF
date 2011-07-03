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

        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<T, TFuture> future)
        {
            return Reference.BuildParser(future);
        }
    }
}
