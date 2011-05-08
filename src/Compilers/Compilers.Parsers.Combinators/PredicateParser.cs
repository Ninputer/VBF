using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers.Combinators
{
    public class PredicateParser<T> : Parser<T>
    {
        public Parser<T> Parser { get; private set; }
        public Func<T, bool> Predicate { get; private set; }

        public PredicateParser(Parser<T> parser, Func<T, bool> predicate)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(predicate, "predicate");

            Parser = parser;
            Predicate = predicate;
        }

        public override Func<Scanners.ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) => Parser.Run<TFuture>(v => Predicate(v) ? future(v) : (s,c) => c.FailResult<TFuture>())(scanner, context);
        }
    }
}
