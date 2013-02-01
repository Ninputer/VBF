using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public static class Grammar
    {
        public static ProductionBase<Lexeme> AsTerminal(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return Terminal.GetTerminal(token);
        }

        public static ProductionBase<Lexeme> Eos()
        {
            return new EndOfStream();
        }

        public static ProductionBase<T> Empty<T>(T value)
        {
            return new EmptyProduction<T>(value);
        }

        public static ProductionBase<TResult> Select<TSource, TResult>(this ProductionBase<TSource> production, Func<TSource, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingProduction<TSource, TResult>(production, selector);
        }

        public static ProductionBase<TResult> Select<TResult>(this Token token, Func<Lexeme, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingProduction<Lexeme, TResult>(token.AsTerminal(), selector);
        }

        public static ProductionBase<TResult> SelectMany<T1, T2, TResult>(this ProductionBase<T1> production, Func<T1, ProductionBase<T2>> productionSelector, Func<T1, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<T1, T2, TResult>(production, productionSelector, resultSelector);
        }

        public static ProductionBase<TResult> SelectMany<T2, TResult>(this Token token, Func<Lexeme, ProductionBase<T2>> productionSelector, Func<Lexeme, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<Lexeme, T2, TResult>(token.AsTerminal(), productionSelector, resultSelector);
        }

        public static ProductionBase<TResult> SelectMany<T1, TResult>(this ProductionBase<T1> production, Func<T1, Token> productionSelector, Func<T1, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<T1, Lexeme, TResult>(production, v => productionSelector(v).AsTerminal(), resultSelector);
        }

        public static ProductionBase<TResult> SelectMany<TResult>(this Token token, Func<Lexeme, Token> productionSelector, Func<Lexeme, Lexeme, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(productionSelector, "productionSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            return new ConcatenationProduction<Lexeme, Lexeme, TResult>(token.AsTerminal(), v => productionSelector(v).AsTerminal(), resultSelector);
        }

        public static ProductionBase<T> Union<T>(this ProductionBase<T> production1, ProductionBase<T> production2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            return new AlternationProduction<T>(production1, production2);
        }

        public static ProductionBase<Tuple<T1, T2>> Concat<T1, T2>(this ProductionBase<T1> production1, ProductionBase<T2> production2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            return from v1 in production1 from v2 in production2 select Tuple.Create(v1, v2);
        }

        public static ProductionBase<Tuple<Lexeme, T2>> Concat<T2>(this Token token1, ProductionBase<T2> production2)
        {
            CodeContract.RequiresArgumentNotNull(token1, "token1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            return from v1 in token1 from v2 in production2 select Tuple.Create(v1, v2);
        }

        public static ProductionBase<Tuple<T1, Lexeme>> Concat<T1>(this ProductionBase<T1> production1, Token token2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(token2, "token2");

            return from v1 in production1 from v2 in token2 select Tuple.Create(v1, v2);
        }

        public static ProductionBase<Tuple<Lexeme, Lexeme>> Concat(this Token token1, Token token2)
        {
            CodeContract.RequiresArgumentNotNull(token1, "token1");
            CodeContract.RequiresArgumentNotNull(token2, "token2");

            return from v1 in token1 from v2 in token2 select Tuple.Create(v1, v2);
        }

        public static ProductionBase<T1> First<T1, T2>(this ProductionBase<Tuple<T1, T2>> tupleProduction)
        {
            CodeContract.RequiresArgumentNotNull(tupleProduction, "tupleProduction");

            return tupleProduction.Select(t => t.Item1);
        }

        public static ProductionBase<T2> Second<T1, T2>(this ProductionBase<Tuple<T1, T2>> tupleProduction)
        {
            CodeContract.RequiresArgumentNotNull(tupleProduction, "tupleProduction");

            return tupleProduction.Select(t => t.Item2);
        }

        public static ProductionBase<IEnumerable<T>> Many<T>(this ProductionBase<T> production)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many1().Union(Empty(new RepeatParserListNode<T>() as IEnumerable<T>));
        }

        public static ProductionBase<IEnumerable<T>> Many<T, TSeparator>(this ProductionBase<T> production, ProductionBase<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many1(separator).Union(Empty(new RepeatParserListNode<T>() as IEnumerable<T>));
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many();
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many<T, TSeparator>(this Token token, ProductionBase<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many(separator);
        }

        public static ProductionBase<IEnumerable<T>> Many<T>(this ProductionBase<T> production, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many(separator.AsTerminal());
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many<T, TSeparator>(this Token token, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many(separator.AsTerminal());
        }

        public static ProductionBase<IEnumerable<T>> Many1<T>(this ProductionBase<T> production)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return from r in production
                   from rm in production.Many()
                   select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many1(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many1();
        }

        public static ProductionBase<IEnumerable<T>> Many1<T, TSeparator>(this ProductionBase<T> production, ProductionBase<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return from r in production
                   from rm in production.PrefixedBy(separator).Many()
                   select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;
        }

        public static ProductionBase<IEnumerable<T>> Many1<T>(this ProductionBase<T> production, Token seperator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many1(seperator.AsTerminal());
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many1<TSeparator>(this Token token, ProductionBase<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many1(separator);
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many1(this Token token, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many1(separator.AsTerminal());
        }

        public static ProductionBase<T> Optional<T>(this ProductionBase<T> production)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Union(Empty(default(T)));
        }

        public static ProductionBase<Lexeme> Optional(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Optional();
        }

        public static ProductionBase<T> PrefixedBy<T, TPrefix>(this ProductionBase<T> production, ProductionBase<TPrefix> prefix)
        {
            return prefix.Concat(production).Second();
        }

        public static ProductionBase<T> PrefixedBy<T>(this ProductionBase<T> production, Token prefix)
        {
            return prefix.Concat(production).Second();
        }

        public static ProductionBase<T> SuffixedBy<T, TSuffix>(this ProductionBase<T> parser, ProductionBase<TSuffix> suffix)
        {
            return parser.Concat(suffix).First();
        }

        public static ProductionBase<T> SuffixedBy<T>(this ProductionBase<T> parser, Token suffix)
        {
            return parser.Concat(suffix).First();
        }
    }
}
