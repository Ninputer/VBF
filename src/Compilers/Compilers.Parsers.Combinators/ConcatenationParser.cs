using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ConcatenationParser<T1, T2, TR> : Parser<TR>
    {
        public Parser<T1> ParserLeft { get; private set; }
        public Func<T1, Parser<T2>> ParserRightSelector { get; private set; }
        public Func<T1, T2, TR> Selector { get; private set; }

        public ConcatenationParser(Parser<T1> parserLeft, Func<T1, Parser<T2>> parserRightSelector, Func<T1, T2, TR> selector)
        {
            CodeContract.RequiresArgumentNotNull(parserLeft, "parserLeft");
            CodeContract.RequiresArgumentNotNull(parserRightSelector, "parserRightSelector");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            ParserLeft = parserLeft;
            ParserRightSelector = parserRightSelector;
            Selector = selector;
        }

        public override Func<ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<TR, TFuture> future)
        {
            return (scanner, context) => ParserLeft.Run(left => ParserRightSelector(left).Run(right => future(Selector(left, right))))(scanner, context);
        }
    }
}
