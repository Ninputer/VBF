// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public static class Parsers
    {
        public static Parser<Lexeme> AsParser(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return new TokenParser(token, null);
        }

        public static Parser<Lexeme> AsParser(this Token token, int lexerStateIndex)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return new TokenParser(token, lexerStateIndex);
        }

        public static Parser<Lexeme> Eos()
        {
            return new EndOfStreamParser();
        }

        public static Parser<T> Succeed<T>(T value)
        {
            return new SucceedParser<T>(value);
        }

        public static Parser<Lexeme> Any()
        {
            return new AnyTokenParser();
        }

        public static Parser<TResult> Select<TSource, TResult>(this Parser<TSource> parser, Func<TSource, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingParser<TSource, TResult>(parser, selector);
        }

        public static Parser<TResult> Select<TResult>(this Token token, Func<Lexeme, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingParser<Lexeme, TResult>(token.AsParser(), selector);
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

        public static Parser<Lexeme> Where(this Token token, Func<Lexeme, bool> predicate)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(predicate, "predicate");

            return new TokenParser(token, null, predicate);
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

        public static Parser<IEnumerable<T>> Many<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Many1().Union(Succeed(new RepeatParserListNode<T>() as IEnumerable<T>));
        }

        public static Parser<IEnumerable<T>> Many<T, TSeparator>(this Parser<T> parser, Parser<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Many1(separator).Union(Succeed(new RepeatParserListNode<T>() as IEnumerable<T>));
        }

        public static Parser<IEnumerable<Lexeme>> Many(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Many();
        }

        public static Parser<IEnumerable<Lexeme>> Many<TSeparator>(this Token token, Parser<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "parser");

            return token.AsParser().Many(separator);
        }

        public static Parser<IEnumerable<T>> Many<T>(this Parser<T> parser, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Many(separator.AsParser());
        }

        public static Parser<IEnumerable<Lexeme>> Many(this Token token, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "parser");

            return token.AsParser().Many(separator.AsParser());
        }

        public static Parser<IEnumerable<T>> Many1<T>(this Parser<T> parser)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return from r in parser
                   from rm in parser.Many()
                   select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;
        }

        public static Parser<IEnumerable<Lexeme>> Many1(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsParser().Many1();
        }

        public static Parser<IEnumerable<T>> Many1<T, TSeparator>(this Parser<T> parser, Parser<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return from r in parser
                   from rm in parser.PrefixedBy(separator).Many()
                   select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;
        }

        public static Parser<IEnumerable<T>> Many1<T>(this Parser<T> parser, Token seperator)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");

            return parser.Many1(seperator.AsParser());
        }

        public static Parser<IEnumerable<Lexeme>> Many1<TSeparator>(this Token token, Parser<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "parser");

            return token.AsParser().Many1(separator);
        }

        public static Parser<IEnumerable<Lexeme>> Many1(this Token token, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "parser");

            return token.AsParser().Many1(separator.AsParser());
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

        public static Parser<T> PrefixedBy<T>(this Parser<T> parser, Token prefix)
        {
            return prefix.Concat(parser).Second();
        }

        public static Parser<T> SuffixedBy<T, TSuffix>(this Parser<T> parser, Parser<TSuffix> suffix)
        {
            return parser.Concat(suffix).First();
        }

        public static Parser<T> SuffixedBy<T>(this Parser<T> parser, Token suffix)
        {
            return parser.Concat(suffix).First();
        }

        public static Parser<T> PackedBy<T, TPrefix, TSuffix>(this Parser<T> parser, Parser<TPrefix> prefix, Parser<TSuffix> suffix)
        {
            return prefix.Concat(parser).Second().Concat(suffix).First();
        }

        public static Parser<T> PackedBy<T>(this Parser<T> parser, Token prefix, Token suffix)
        {
            return prefix.Concat(parser).Second().Concat(suffix).First();
        }
    }
}
