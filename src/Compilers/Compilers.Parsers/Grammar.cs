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
using System.Linq.Expressions;
using System.Reflection;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    /// <summary>
    /// Provides a set of static (Shared in Visual Basic) method to build parser grammar using Linq query expressions.
    /// </summary>
    public static class Grammar
    {
        private static MethodInfo s_checkMethod;

        static Grammar()
        {
            s_checkMethod = typeof(Grammar).GetMethod("Check", BindingFlags.Static | BindingFlags.Public);
        }

        /// <summary>
        /// Converts a <see cref="VBF.Compilers.Scanners.Token"/> to a terminal production.
        /// </summary>
        /// <param name="token">The <see cref="VBF.Compilers.Scanners.Token"/> used as a terminal</param>
        /// <returns>The production object represents the terminal</returns>
        public static ProductionBase<Lexeme> AsTerminal(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return Terminal.GetTerminal(token);
        }

        /// <summary>
        /// Generates a production of 'end of stream' terminal. Note: this production is added automatically by <see cref="ParserBase{T}"/> class 
        /// to the end of your root production. In normal case, you don't need to use this production manually. 
        /// </summary>
        /// <returns>The production object represents the 'end of stream' terminal.</returns>
        public static ProductionBase<Lexeme> Eos()
        {
            return EndOfStream.Instance;
        }

        /// <summary>
        /// Generates a production that doesn't consume any token but results a specific object in parsing.
        /// </summary>
        /// <typeparam name="T">The type of parsing result object</typeparam>
        /// <param name="value">The parsing result object of this production</param>
        /// <returns>The generated 'empty' production object</returns>
        public static ProductionBase<T> Empty<T>(T value)
        {
            return new EmptyProduction<T>(value);
        }

        /// <summary>
        /// Generates a production that converts the parsing result of another production using a specific converting rule. 
        /// In other words, generates a 'mapping' production
        /// </summary>
        /// <typeparam name="TSource">The type of the parsing result of the source production</typeparam>
        /// <typeparam name="TResult">The type of the converted parsing result</typeparam>
        /// <param name="production">The source production whose parsing result will be converted</param>
        /// <param name="selector">The delegate that represents the converting rule</param>
        /// <returns>The generated 'mapping' production object</returns>
        /// <example>
        /// The following code converts the parsing result of <see cref="Production{int}"/> into string from int.
        /// <code language="C#">
        /// Production&lt;int&gt; intProduction = new Production&lt;int&gt;();
        /// 
        /// var stringProduction =
        ///     from iv in intProduction
        ///     select iv.ToString();
        /// </code>
        /// </example>
        public static ProductionBase<TResult> Select<TSource, TResult>(this ProductionBase<TSource> production, Func<TSource, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingProduction<TSource, TResult>(production, selector);
        }

        /// <summary>
        /// Generates a production that converts the parsing result of a terminal using a specific converting rule. 
        /// In other words, generates a 'mapping' production from a token
        /// </summary>
        /// <typeparam name="TResult">The type of the converted parsing result</typeparam>
        /// <param name="token">The token used as the source terminal production whose parsing result will be converted.</param>
        /// <param name="selector">The delegate that represents the converting rule</param>
        /// <returns>The generated 'mapping' production object</returns>
        /// <example>
        /// The following code converts the parsing result of a terminal into string with some formatting.
        /// <code language="C#">
        /// Token T; //initialized in somewhere
        /// 
        /// var formatting =
        ///     from z in T
        ///     select String.Format("Token is {0}", z.Value);
        /// </code>
        /// </example>
        public static ProductionBase<TResult> Select<TResult>(this Token token, Func<Lexeme, TResult> selector)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            return new MappingProduction<Lexeme, TResult>(token.AsTerminal(), selector);
        }

        /// <summary>
        /// Generates a production that concatenates two productions. The generated 'concatenation' production consumes the input of two source productions in the order, 
        /// and then using a converting rule to convert the parsing results of two source productions. In practice, this method is used to build multiple embedded 'from' 
        /// clauses in a Linq query expression to concatenate multiple source productions. 
        /// </summary>
        /// <typeparam name="T1">The type of the parsing result of the 1st source production</typeparam>
        /// <typeparam name="T2">The type of the parsing result of the 2nd source production</typeparam>
        /// <typeparam name="TResult">The parsing result of the 'concatentation' production</typeparam>
        /// <param name="production">The 1st source production</param>
        /// <param name="productionSelector">The delegate used to generated the 2nd source production. 
        /// Note that the argument of this delegate will be always null (Nothing in Visual Basic)</param>
        /// <param name="resultSelector">The delegate that represents the converting rule of parsing result</param>
        /// <returns>The generated 'concatentation' production object</returns>
        /// <example>
        /// The following code concatenates two <see cref="Production{T}"/> and formats the parsing result.
        /// <code language="C#">
        /// Production&lt;string&gt; strProduction = new Production&lt;string&gt;();
        /// Production&lt;int&gt; intProduction = new Production&lt;int&gt;();
        /// 
        /// var concatenation =
        ///     from sv in strProduction
        ///     from iv in intProduction
        ///     select String.Format("{0} is followed by {1}", sv, iv);
        /// </code>
        /// </example>
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

        public static ProductionBase<TResult> Where<TResult>(this ProductionBase<TResult> production, Expression<Func<TResult, bool>> predicate)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");
            CodeContract.RequiresArgumentNotNull(predicate, "predicate");

            Expression<Func<TResult, bool>> finalRule = null;
            Expression<Func<TResult, SourceSpan>> positionGetter = null;
            int? errorId = null;

            //exptract the first layer of 'predicate'
            if (predicate.Body.NodeType == ExpressionType.Call)
            {
                MethodCallExpression call = predicate.Body as MethodCallExpression;

                if (call.Method.Equals(s_checkMethod))
                {
                    //it is the call to Grammar.Check, extract message

                    var ruleExp = call.Arguments[0];
                    var errIdExp = call.Arguments[1];
                    var positionExp = call.Arguments[2];

                    positionGetter = Expression.Lambda<Func<TResult, SourceSpan>>(positionExp, predicate.Parameters[0]);
                    Expression<Func<int>> idGetter = Expression.Lambda<Func<int>>(errIdExp);

                    errorId = idGetter.Compile()();

                    finalRule = Expression.Lambda<Func<TResult, bool>>(ruleExp, predicate.Parameters[0]);
                }
            }

            if (finalRule == null)
            {
                finalRule = predicate;
            }

            return new MappingProduction<TResult, TResult>(production, x => x, finalRule.Compile(), errorId, positionGetter != null ? positionGetter.Compile() : null);
        }

        public static ProductionBase<Lexeme> Where(this Token token, Expression<Func<Lexeme, bool>> predicate)
        {
            return token.AsTerminal().Where(predicate);
        }

        public static bool Check(bool condition, int errorId, SourceSpan position)
        {
            throw new InvalidOperationException("This method is only used in the 'where' clause of parser Linq expressions");
        }

        public static ProductionBase<T> Union<T>(this ProductionBase<T> production1, ProductionBase<T> production2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            return new AlternationProduction<T>(production1, production2);
        }

        public static ProductionBase<T> Union<T>(params ProductionBase<T>[] productions)
        {
            CodeContract.RequiresArgumentInRange(productions.Length > 0, "productions", "There must be at least one production to be unioned");

            ProductionBase<T> result = productions[0];

            for (int i = 1; i < productions.Length; i++)
            {
                result = new AlternationProduction<T>(result, productions[i]);
            }

            return result;
        }

        public static ProductionBase<Lexeme> Union(params Token[] tokens)
        {
            CodeContract.RequiresArgumentInRange(tokens.Length > 0, "tokens", "There must be at least on token to be unioned");

            ProductionBase<Lexeme> result = tokens[0].AsTerminal();

            for (int i = 1; i < tokens.Length; i++)
            {
                result = new AlternationProduction<Lexeme>(result, tokens[i].AsTerminal());
            }

            return result;
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

            Production<IEnumerable<T>> many1 = new Production<IEnumerable<T>>();
            var many = Empty(new RepeatParserListNode<T>() as IEnumerable<T>) | many1;


            many1.Rule = from r in production
                         from rm in many
                         select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;

            return many;
        }

        public static ProductionBase<IEnumerable<T>> Many<T, TSeparator>(this ProductionBase<T> production, ProductionBase<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return Empty(new RepeatParserListNode<T>() as IEnumerable<T>).Union(production.Many1(separator));
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many();
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many<TSeparator>(this Token token, ProductionBase<TSeparator> separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many(separator);
        }

        public static ProductionBase<IEnumerable<T>> Many<T>(this ProductionBase<T> production, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            return production.Many(separator.AsTerminal());
        }

        public static ProductionBase<IEnumerable<Lexeme>> Many(this Token token, Token separator)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            return token.AsTerminal().Many(separator.AsTerminal());
        }

        public static ProductionBase<IEnumerable<T>> Many1<T>(this ProductionBase<T> production)
        {
            CodeContract.RequiresArgumentNotNull(production, "production");

            Production<IEnumerable<T>> many1 = new Production<IEnumerable<T>>();
            var many = Empty(new RepeatParserListNode<T>() as IEnumerable<T>) | many1;


            many1.Rule = from r in production
                         from rm in many
                         select new RepeatParserListNode<T>(r, rm as RepeatParserListNode<T>) as IEnumerable<T>;

            return many1;
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
            return from pr in prefix from p in production select p;
        }

        public static ProductionBase<T> PrefixedBy<T>(this ProductionBase<T> production, Token prefix)
        {
            return from pr in prefix from p in production select p;
        }

        public static ProductionBase<T> SuffixedBy<T, TSuffix>(this ProductionBase<T> production, ProductionBase<TSuffix> suffix)
        {
            return from p in production from s in suffix select p;
        }

        public static ProductionBase<T> SuffixedBy<T>(this ProductionBase<T> production, Token suffix)
        {
            return from p in production from s in suffix select p;
        }

        public static ProductionBase<T> PackedBy<T, TPrefix, TSuffix>(this ProductionBase<T> production, ProductionBase<TPrefix> prefix, ProductionBase<TSuffix> suffix)
        {
            return from pr in prefix from p in production from s in suffix select p;
        }

        public static ProductionBase<T> PackedBy<T>(this ProductionBase<T> production, Token prefix, Token suffix)
        {
            return from pr in prefix from p in production from s in suffix select p;
        }
    }
}
