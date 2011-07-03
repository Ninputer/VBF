using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class AlternationParser<T> : Parser<T>
    {
        public Parser<T> Parser1 { get; private set; }
        public Parser<T> Parser2 { get; private set; }

        public AlternationParser(Parser<T> parser1, Parser<T> parser2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            Parser1 = parser1;
            Parser2 = parser2;
        }

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
