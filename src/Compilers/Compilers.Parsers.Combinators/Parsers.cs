using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public static class Parsers
    {
        public static Parser<T> Wrap<T>(ParserFunc<T> rule)
        {
            CodeContract.RequiresArgumentNotNull(rule, "rule");

            return new Parser<T>(rule);
        }

        public static Parser<Lexeme> AsParser(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return Wrap(scanner =>
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
            });
        }

        public static Parser<Lexeme> AsParser(this Token token, int lexerStateIndex)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return Wrap(scanner =>
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
            });
        }

        public static IResult<Lexeme> Scan(this Token token, ForkableScanner scanner)
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
        }

        public static Parser<T> Success<T>(T result)
        {
            return Wrap(scanner => new Result<T>(result));
        }

        public static Parser<T> Fail<T>()
        {
            return Wrap<T>(scanner => null);
        }

        public static Parser<Lexeme> Any()
        {
            return Wrap(scanner => new Result<Lexeme>(scanner.Read()));
        }

        public static Parser<T> Union<T>(this Parser<T> parser1, Parser<T> parser2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            return Wrap(scanner =>
            {
                var scanner1 = scanner;
                var scanner2 = scanner.Fork();

                var result1 = parser1.Rule(scanner1);
                if (result1 != null)
                {
                    scanner.Join(scanner1);
                    return result1;
                }

                var result2 = parser2.Rule(scanner2);

                scanner.Join(scanner2);
                return result2;
            });
        }

        public static Parser<Tuple<T1, T2>> Concat<T1, T2>(this Parser<T1> parser1, Parser<T2> parser2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            return Wrap(scanner =>
            {
                var result1 = parser1.Rule(scanner);

                if (result1 == null) return null;

                var result2 = parser2.Rule(scanner);

                if (result2 == null) return null;

                return new Result<Tuple<T1, T2>>(Tuple.Create(result1.Value, result2.Value));
            });
        }

        public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(predicate, "predicate");

            return Wrap(scanner =>
            {
                var result = parser.Rule(scanner);

                if (predicate(result.Value))
                {
                    return result;
                }
                else
                {
                    return null;
                }
            });
        }

        public static Parser<Lexeme> Where(this Token token, Func<Lexeme, bool> predicate)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(predicate, "predicate");

            return Wrap(scanner =>
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
            });
        }

        public static Parser<TResult> Select<TSource, TResult>(this Parser<TSource> parser, Func<TSource, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return Wrap(scanner => 
            {
                var result = parser.Rule(scanner);

                if (result == null) return null;

                return new Result<TResult>(selector(result.Value));
            });
        }

        public static Parser<TResult> Select<TResult>(this Token token, Func<Lexeme, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return token.AsParser().Select(selector);
        }

        public static Parser<TResult> SelectMany<T1, T2, TResult>(this Parser<T1> parser, Func<T1, Parser<T2>> parserSelector, Func<T1, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return Wrap(scanner =>
            {
                var result1 = parser.Rule(scanner);

                if (result1 == null) return null;

                var parser2 = parserSelector(result1.Value);

                var result2 = parser2.Rule(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            });
        }

        public static Parser<TResult> SelectMany<T, TResult>(this Token token, Func<Lexeme, Parser<T>> parserSelector, Func<Lexeme, T, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return Wrap(scanner =>
            {
                var result1 = token.Scan(scanner);

                if (result1 == null) return null;

                var parser2 = parserSelector(result1.Value);

                var result2 = parser2.Rule(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            });
        }

        public static Parser<TResult> SelectMany<T, TResult>(this Parser<T> parser, Func<T, Token> parserSelector, Func<T, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return Wrap(scanner =>
            {
                var result1 = parser.Rule(scanner);

                if (result1 == null) return null;

                var token2 = parserSelector(result1.Value);

                var result2 = token2.Scan(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            });
        }

        public static Parser<TResult> SelectMany<TResult>(this Token token, Func<Lexeme, Token> parserSelector, Func<Lexeme, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return Wrap(scanner =>
            {
                var result1 = token.Scan(scanner);

                if (result1 == null) return null;

                var token2 = parserSelector(result1.Value);

                var result2 = token2.Scan(scanner);

                if (result2 == null) return null;

                return new Result<TResult>(resultSelector(result1.Value, result2.Value));
            });
        }

        public static Parser<T[]> Many<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Many1().Union(Success(new T[0]));
        }

        public static Parser<T[]> Many1<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return from r in parser
                   from rm in parser.Many()
                   select new[] { r }.Concat(rm).ToArray();
        }

        public static Parser<Lexeme[]> Many(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Many();
        }

        public static Parser<Lexeme[]> Many1(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Many1();
        }

        public static Parser<T> Optional<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Union(Success(default(T)));
        }

        public static Parser<Lexeme> Optional(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Optional();
        }
    }
}
