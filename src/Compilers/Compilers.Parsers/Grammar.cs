using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public static class Grammar
    {
        public static Production<Lexeme> AsTerminal(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return new Terminal(token);
        }

        public static Production<Lexeme> Eos()
        {
            return new EndOfStream();
        }

        public static Production<T> Empty<T>(T value)
        {
            return new EmptyProduction<T>(value);
        }

        public static Production<TResult> Select<TSource, TResult>(this Production<TSource> production, Func<TSource, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingProduction<TSource, TResult>(production, selector);
        }

        public static Production<TResult> Select<TResult>(this Token token, Func<Lexeme, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingProduction<Lexeme, TResult>(token.AsTerminal(), selector);
        }

        public static Production<TResult> SelectMany<T1, T2, TResult>(this Production<T1> production, Func<T1, Production<T2>> productionSelector, Func<T1, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<T1, T2, TResult>(production, productionSelector, resultSelector);
        }

        public static Production<TResult> SelectMany<T2, TResult>(this Token token, Func<Lexeme, Production<T2>> productionSelector, Func<Lexeme, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<Lexeme, T2, TResult>(token.AsTerminal(), productionSelector, resultSelector);
        }

        public static Production<TResult> SelectMany<T1, TResult>(this Production<T1> production, Func<T1, Token> productionSelector, Func<T1, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<T1, Lexeme, TResult>(production, v => productionSelector(v).AsTerminal(), resultSelector);
        }

        public static Production<TResult> SelectMany<TResult>(this Token token, Func<Lexeme, Token> productionSelector, Func<Lexeme, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<Lexeme, Lexeme, TResult>(token.AsTerminal(), v => productionSelector(v).AsTerminal(), resultSelector);
        }

        public static Production<T> Union<T>(this Production<T> production1, Production<T> production2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            return new AlternationProduction<T>(production1, production2);
        }

        public static Production<Tuple<T1, T2>> Concat<T1, T2>(this Production<T1> production1, Production<T2> production2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            return from v1 in production1 from v2 in production2 select Tuple.Create(v1, v2);
        }

        public static Production<Tuple<Lexeme, T2>> Concat<T2>(this Token token1, Production<T2> production2)
        {
            CodeContract.RequiresArgumentNotNull(token1, "token1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            return from v1 in token1 from v2 in production2 select Tuple.Create(v1, v2);
        }

        public static Production<Tuple<T1, Lexeme>> Concat<T1>(this Production<T1> production1, Token token2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(token2, "token2");

            return from v1 in production1 from v2 in token2 select Tuple.Create(v1, v2);
        }

        public static Production<Tuple<Lexeme, Lexeme>> Concat(this Token token1, Token token2)
        {
            CodeContract.RequiresArgumentNotNull(token1, "token1");
            CodeContract.RequiresArgumentNotNull(token2, "token2");

            return from v1 in token1 from v2 in token2 select Tuple.Create(v1, v2);
        }

        public static Production<T1> First<T1, T2>(this Production<Tuple<T1, T2>> tupleProduction)
        {
            CodeContract.RequiresArgumentNotNull(tupleProduction, "tupleProduction");

            return tupleProduction.Select(t => t.Item1);
        }

        public static Production<T2> Second<T1, T2>(this Production<Tuple<T1, T2>> tupleProduction)
        {
            CodeContract.RequiresArgumentNotNull(tupleProduction, "tupleProduction");

            return tupleProduction.Select(t => t.Item2);
        }

        public static Production<IEnumerable<T>> Many<T>(this Production<T> production)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many1().Union(Empty(new RepeatParserListNode<T>() as IEnumerable<T>));
        }

        public static Production<IEnumerable<T>> Many<T, TSeparator>(this Production<T> production, Production<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many1(separator).Union(Empty(new RepeatParserListNode<T>() as IEnumerable<T>));
        }

        public static Production<IEnumerable<Lexeme>> Many(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many();
        }

        public static Production<IEnumerable<Lexeme>> Many<T, TSeparator>(this Token token, Production<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many(separator);
        }

        public static Production<IEnumerable<T>> Many<T>(this Production<T> production, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many(separator.AsTerminal());
        }

        public static Production<IEnumerable<Lexeme>> Many<T, TSeparator>(this Token token, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many(separator.AsTerminal());
        }

        public static Production<IEnumerable<T>> Many1<T>(this Production<T> production)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return from r in production
                   from rm in production.Many()
                   select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;
        }

        public static Production<IEnumerable<Lexeme>> Many1(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many1();
        }

        public static Production<IEnumerable<T>> Many1<T, TSeparator>(this Production<T> production, Production<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return from r in production
                   from rm in production.PrefixedBy(separator).Many()
                   select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;
        }

        public static Production<IEnumerable<T>> Many1<T>(this Production<T> production, Token seperator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many1(seperator.AsTerminal());
        }

        public static Production<IEnumerable<Lexeme>> Many1<TSeparator>(this Token token, Production<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many1(separator);
        }

        public static Production<IEnumerable<Lexeme>> Many1(this Token token, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many1(separator.AsTerminal());
        }

        public static Production<T> Optional<T>(this Production<T> production)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Union(Empty(default(T)));
        }

        public static Production<Lexeme> Optional(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Optional();
        }

        public static Production<T> PrefixedBy<T, TPrefix>(this Production<T> production, Production<TPrefix> prefix)
        {
            return prefix.Concat(production).Second();
        }

        public static Production<T> PrefixedBy<T>(this Production<T> production, Token prefix)
        {
            return prefix.Concat(production).Second();
        }
    }
}
