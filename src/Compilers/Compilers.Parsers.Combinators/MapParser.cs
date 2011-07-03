using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class MapParser<TSource, TReturn> : Parser<TReturn>
    {
        public Parser<TSource> SourceParser { get; private set; }
        public Func<TSource, TReturn> Selector { get; private set; }

        public MapParser(Parser<TSource> sourceParser, Func<TSource, TReturn> selector)
        {
            CodeContract.RequiresArgumentNotNull(sourceParser, "sourceParser");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            SourceParser = sourceParser;
            Selector = selector;
        }

        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<TReturn, TFuture> future)
        {
            return (scanner, context) => SourceParser.BuildParser(vsource => future(Selector(vsource)))(scanner, context);
        }
    }
}
