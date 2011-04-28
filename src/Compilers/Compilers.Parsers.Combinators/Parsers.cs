using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public static class Parsers
    {
        public static Parser<Lexeme> AsParser(this Token token)
        {
            return scanner =>
            {
                var lexeme = scanner.Read();
                if (lexeme.TokenIndex == token.Index)
                {
                    return new Result<Lexeme>(lexeme);
                }
                else
                {
                    return null;
                }
            };
        }

        public static Parser<Lexeme> AsParser(this Token token, int lexerStateIndex)
        {
            return scanner =>
            {
                var lexeme = scanner.Read();
                if (lexeme.GetTokenIndex(lexerStateIndex) == token.Index)
                {
                    return new Result<Lexeme>(lexeme);
                }
                else
                {
                    return null;
                }
            };
        }

        public static Result<Lexeme> Scan(this Token token, ForkableScanner scanner)
        {
            return token.AsParser()(scanner);
        }

        public static Parser<T> Success<T>(T result)
        {
            return scanner => new Result<T>(result);
        }

        public static Parser<T> Fail<T>()
        {
            return scanner => null;
        }

        public static Parser<Lexeme> Any()
        {
            return scanner => new Result<Lexeme>(scanner.Read());
        }

        public static Parser<T> Or<T>(this Parser<T> parser1, Parser<T> parser2)
        {
            return scanner =>
            {
                var scanner1 = scanner;
                var scanner2 = scanner.Fork();

                var result1 = parser1(scanner1);
                if (result1 != null)
                {
                    scanner2.Close();
                    return result1;
                }
                else
                {
                    scanner1.Close();
                }

                var result2 = parser2(scanner2);
                return result2;
            };
        }

        public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate)
        {
            return scanner =>
            {
                var result = parser(scanner);

                if (predicate(result.Value))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            };
        }

        public static Parser<Lexeme> Where(this Token token, Func<Lexeme, bool> predicate)
        {
            return scanner =>
            {
                var result = token.Scan(scanner);

                if (predicate(result.Value))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            };
        }

        public static Parser<TResult> Select<TSource, TResult>(this Parser<TSource> parser, Func<TSource, TResult> selector)
        {
            return scanner => new Result<TResult>(selector(parser(scanner).Value));
        }

        public static Parser<TResult> Select<TResult>(this Token token, Func<Lexeme, TResult> selector)
        {
            return scanner => new Result<TResult>(selector(token.Scan(scanner).Value));
        }

        public static Parser<TResult> SelectMany<T1, T2, TResult>(this Parser<T1> parser, Func<T1, Parser<T2>> parserSelector, Func<T1, T2, TResult> resultSelector)
        {
            return scanner =>
            {
                var result1 = parser(scanner);

                if (result1 == null) return null;

                var parser2 = parserSelector(result1.Value);

                var result2 = parser2(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            };
        }

        public static Parser<TResult> SelectMany<T, TResult>(this Token token, Func<Lexeme, Parser<T>> parserSelector, Func<Lexeme, T, TResult> resultSelector)
        {
            return scanner =>
            {
                var result1 = token.Scan(scanner);

                if (result1 == null) return null;

                var parser2 = parserSelector(result1.Value);

                var result2 = parser2(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            };
        }

        public static Parser<TResult> SelectMany<T, TResult>(this Parser<T> parser, Func<T, Token> parserSelector, Func<T, Lexeme, TResult> resultSelector)
        {
            return scanner =>
            {
                var result1 = parser(scanner);

                if (result1 == null) return null;

                var token2 = parserSelector(result1.Value);

                var result2 = token2.Scan(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            };
        }

        public static Parser<TResult> SelectMany<TResult>(this Token token, Func<Lexeme, Token> parserSelector, Func<Lexeme, Lexeme, TResult> resultSelector)
        {
            return scanner =>
            {
                var result1 = token.Scan(scanner);

                if (result1 == null) return null;

                var token2 = parserSelector(result1.Value);

                var result2 = token2.Scan(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            };
        }

        public static Parser<T[]> Many<T>(this Parser<T> parser)
        {
            return parser.Many1().Or(Success(new T[0]));
        }

        public static Parser<T[]> Many1<T>(this Parser<T> parser)
        {
            return from r in parser
                   from rm in parser.Many()
                   select new[] { r }.Concat(rm).ToArray();
        }

        public static Parser<Lexeme[]> Many(this Token token)
        {
            return token.AsParser().Many();
        }

        public static Parser<Lexeme[]> Many1(this Token token)
        {
            return token.AsParser().Many1();
        }
    }
}
