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

        public static Parser<T> operator |(Parser<T> p1, Parser<T> p2)
        {
            return new Alternation<T>(p1, p2);
        }
    }

    public class Succeed<T> : Parser<T>
    {
        public T Value { get; private set; }

        public Succeed(T value)
        {
            Value = value;
        }
        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return scanner => future(Value)(scanner);
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
            return scanner => ParserLeft.Run(left => ParserRight.Run(right => future(Tuple.Create(left, right))))(scanner);
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
            return scanner => ParserLeft.Run(left => ParserRight.Run(right => future(Selector(left, right))))(scanner);
        }
    }

    public class Alternation<T> : Parser<T>
    {
        public Parser<T> Parser1 { get; private set; }
        public Parser<T> Parser2 { get; private set; }

        public Alternation(Parser<T> parser1, Parser<T> parser2)
        {
            Parser1 = parser1;
            Parser2 = parser2;
        }

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

        public TokenParser(Token expected)
        {
            ExpectedToken = expected;
        }

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
                    var r = Result.Step(0, () => future(l)(scanner));
                    return r;
                }
                else
                {
                    if (l.IsEndOfStream)
                    {
                        scanner.Join(s1);
                        return Result.Step<TFuture>(1, () => future(l.GetErrorCorrectionLexeme(ExpectedToken.Index, "<missing>"))(scanner)); //insert
                    }
                    else
                    {
                        return Parsers.Best(
                            Result.Step(1, () => future(l.GetErrorCorrectionLexeme(ExpectedToken.Index, "<missing>"))(scanner)), s1, //insert
                            Result.Step(1, () => scan(s1)), scanner, //delete
                            scanner);
                    }
                }
            };

            return scan;
        }
    }

    public class DelayAssignParser<T> : Parser<T>
    {
        public Parser<T> Rule { get; set; }

        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return Rule.Run(future);
        }
    }

    public class EndOfStreamParser : Parser<Lexeme>
    {
        public override Func<ForkableScanner, Result<TFuture>> Run<TFuture>(Future<Lexeme, TFuture> future)
        {
            return scanner =>
            {
                var s1 = scanner.Fork();

                var l = scanner.Read();

                if (l.IsEndOfStream)
                {
                    //s1.Close();
                    return Result.Step(0, () => future(l)(scanner));
                }
                else
                {
                    //scanner.Join(s1);
                    return Result.Fail<TFuture>();
                }
            };
            throw new NotImplementedException();
        }
    }
}
