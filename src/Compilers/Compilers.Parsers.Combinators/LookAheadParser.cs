using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ChooseBestParser<T> : Parser<T>
    {
        public Parser<T> Parser1 { get; private set; }
        public Parser<T> Parser2 { get; private set; }

        public ChooseBestParser(Parser<T> parser1, Parser<T> parser2)
        {
            Parser1 = parser1;
            Parser2 = parser2;
        }

        public override Func<Scanners.ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) =>
            {
                ForkableScanner s1 = scanner;
                ForkableScanner s2 = scanner;
                return context.ChooseBest(Parser1.Run(future)(s1, context), Parser2.Run(future)(s2, context));
            };
        }
    }

    class LookAheadApplier<T> : ILookAheadConverter<T, LookAhead<T>>
    {
        public Func<Parser<T>, Parser<T>> Converter { get; private set; }

        public LookAhead<T> ConvertShift(Parser<T> parser, Dictionary<int, LookAhead<T>> choices)
        {
            return new Shift<T>(Converter(parser), choices);
        }

        public LookAhead<T> ConvertSplit(LookAhead<T> shift, LookAhead<T> reduce)
        {
            return new Split<T>(shift, reduce);
        }

        public LookAhead<T> ConvertReduce(Parser<T> parser)
        {
            return new Reduce<T>(Converter(parser));
        }

        public LookAhead<T> ConvertFound(Parser<T> parser, LookAhead<T> next)
        {
            return new Found<T>(Converter(parser), next);
        }
    }
}
