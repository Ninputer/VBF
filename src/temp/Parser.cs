using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace ContinuationParserCombinators
{

    public delegate Func<ForkableScanner, Result<TFuture>> Future<T, TFuture>(T value);

    public abstract class Parser<T>
    {
        public abstract Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future);
    }

    public class Succeed<T> : Parser<T>
    {
        public T Value { get; private set; }
        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return future(Value);
        }
    }

    public class Concatenation<T1, T2> : Parser<Tuple<T1, T2>>
    {
        public Parser<T1> ParserLeft { get; private set; }
        public Parser<T2> ParserRight { get; private set; }

        public Concatenation(Parser<T1> parserLeft, Parser<T2> parserRight)
        {
            ParserLeft = parserLeft;
            ParserRight = parserRight;
        }

        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<Tuple<T1, T2>, TFuture> future)
        {
            return ParserLeft.Run(left => ParserRight.Run(right => future(Tuple.Create(left, right))));
        }
    }

    public class ConcatenationSelect<T1, T2, TR> : Parser<TR>
    {
        public Parser<T1> ParserLeft { get; private set; }
        public Parser<T2> ParserRight { get; private set; }
        public Func<T1, T2, TR> Selector { get; private set; }

        public ConcatenationSelect(Parser<T1> parserLeft, Parser<T2> parserRight, Func<T1, T2, TR> selector)
        {
            ParserLeft = parserLeft;
            ParserRight = parserRight;
            Selector = selector;
        }

        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<TR, TFuture> future)
        {
            return ParserLeft.Run(left => ParserRight.Run(right => future(Selector(left, right))));
        }
    }

    public class Alternation<T> : Parser<T>
    {
        public Parser<T> Parser1 { get; private set; }
        public Parser<T> Parser2 { get; private set; }
        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return scanner =>
            {
                var s1 = scanner;
                var s2 = scanner.Fork();

                return Parsers.Best(Parser1.Run(future)(s1), s1, Parser2.Run(future)(s2), s2, scanner);
            };
        }
    }

    public class TokenParser : Parser<Lexeme>
    {
        public Token ExpectedToken { get; private set; }
        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<Lexeme, TFuture> future)
        {
            Func<ForkableScanner, Result<TFuture>> scan = null;
            scan = scanner =>
            {
                var s1 = scanner.Fork();

                var l = scanner.Read();
                if (l.TokenIndex == ExpectedToken.Index)
                {
                    s1.Close();
                    return new Step<TFuture>(0, future(l)(scanner));
                }
                else
                {
                    if (l.IsEndOfStream)
                    {
                        s1.Close();
                        return new Step<TFuture>(1, future(l.GetErrorCorrectionLexeme(ExpectedToken.Index, "<missing>"))(scanner)); //insert
                    }
                    else
                    {
                        return Parsers.Best(
                            new Step<TFuture>(1, future(l.GetErrorCorrectionLexeme(ExpectedToken.Index, "<missing>"))(scanner)), scanner, //insert
                            new Step<TFuture>(1, scan(s1)), s1, //delete
                            scanner);
                    }
                }
            };

            return scan;
        }
    }
}
