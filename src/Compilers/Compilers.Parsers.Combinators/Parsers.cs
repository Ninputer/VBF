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
            CodeContract.RequiresArgumentNotNull(token, "token");

            return new TokenParser(token);
        }

        public static Parser<Lexeme> Eos()
        {
            return new EndOfStreamParser();
        }

        public static Parser<T> Succeed<T>(T value)
        {
            return new SucceedParser<T>(value);
        }

        public static Parser<T> Fail<T>()
        {
            return new FailParser<T>();
        }

        public static Parser<Lexeme> Any()
        {
            return new AnyTokenParser();
        }

        public static Parser<TResult> Select<TSource, TResult>(this Parser<TSource> parser, Func<TSource, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MapParser<TSource, TResult>(parser, selector);
        }

        public static Parser<TResult> Select<TResult>(this Token token, Func<Lexeme, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MapParser<Lexeme, TResult>(token.AsParser(), selector);
        }

        public static Parser<TResult> SelectMany<T1, T2, TResult>(this Parser<T1> parser, Func<T1, Parser<T2>> parserSelector, Func<T1, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationParser<T1, T2, TResult>(parser, parserSelector, resultSelector);
        }

        public static Parser<TResult> SelectMany<T2, TResult>(this Token token, Func<Lexeme, Parser<T2>> parserSelector, Func<Lexeme, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationParser<Lexeme, T2, TResult>(token.AsParser(), parserSelector, resultSelector);
        }

        public static Parser<TResult> SelectMany<T1, TResult>(this Parser<T1> parser, Func<T1, Token> parserSelector, Func<T1, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationParser<T1, Lexeme, TResult>(parser, v => parserSelector(v).AsParser(), resultSelector);
        }

        public static Parser<TResult> SelectMany<TResult>(this Token token, Func<Lexeme, Token> parserSelector, Func<Lexeme, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationParser<Lexeme, Lexeme, TResult>(token.AsParser(), v => parserSelector(v).AsParser(), resultSelector);
        }

        public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(predicate, "predicate");

            return new PredicateParser<T>(parser, predicate);
        }

        public static Parser<Lexeme> Where(this Token token, Func<Lexeme, bool> predicate)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(predicate, "predicate");

            return new PredicateParser<Lexeme>(token.AsParser(), predicate);
        }

        public static Parser<T> Union<T>(this Parser<T> parser1, Parser<T> parser2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            return new AlternationParser<T>(parser1, parser2);
        }

        public static Parser<Tuple<T1, T2>> Concat<T1, T2>(this Parser<T1> parser1, Parser<T2> parser2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            return from v1 in parser1 from v2 in parser2 select Tuple.Create(v1, v2);
        }

        public static Parser<Tuple<Lexeme, T2>> Concat<T2>(this Token token1, Parser<T2> parser2)
        {
            CodeContract.RequiresArgumentNotNull(token1, "token1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            return from v1 in token1 from v2 in parser2 select Tuple.Create(v1, v2);
        }

        public static Parser<Tuple<T1, Lexeme>> Concat<T1>(this Parser<T1> parser1, Token token2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(token2, "token2");

            return from v1 in parser1 from v2 in token2 select Tuple.Create(v1, v2);
        }

        public static Parser<Tuple<Lexeme, Lexeme>> Concat(this Token token1, Token token2)
        {
            CodeContract.RequiresArgumentNotNull(token1, "token1");
            CodeContract.RequiresArgumentNotNull(token2, "token2");

            return from v1 in token1 from v2 in token2 select Tuple.Create(v1, v2);
        }

        public static Parser<T1> First<T1, T2>(this Parser<Tuple<T1, T2>> tupleParser)
        {
            CodeContract.RequiresArgumentNotNull(tupleParser, "tupleParser");

            return tupleParser.Select(t => t.Item1);
        }

        public static Parser<T2> Second<T1, T2>(this Parser<Tuple<T1, T2>> tupleParser)
        {
            CodeContract.RequiresArgumentNotNull(tupleParser, "tupleParser");

            return tupleParser.Select(t => t.Item2);
        }

        public static Parser<T[]> Many<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Many1().Union(Succeed(new T[0]));
        }

        public static Parser<Lexeme[]> Many(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Many();
        }

        public static Parser<T[]> Many1<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return from r in parser
                   from rm in parser.Many()
                   select new[] { r }.Concat(rm).ToArray();
        }

        public static Parser<Lexeme[]> Many1(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Many1();
        }

        public static Parser<T> Optional<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Union(Succeed(default(T)));
        }

        public static Parser<Lexeme> Optional(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Optional();
        }

        public static Parser<T> PrefixedBy<T, TPrefix>(this Parser<T> parser, Parser<TPrefix> prefix)
        {
            return prefix.Concat(parser).Second();
        }

        public static Parser<T> SuffixedBy<T, TSuffix>(this Parser<T> parser, Parser<TSuffix> suffix)
        {
            return parser.Concat(suffix).First();
        }

        public static Parser<T> PackedBy<T, TPrefix, TSuffix>(this Parser<T> parser, Parser<TPrefix> prefix, Parser<TSuffix> suffix)
        {
            return prefix.Concat(parser).Second().Concat(suffix).First();
        }
    }
}
