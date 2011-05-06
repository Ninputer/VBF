using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;
using VBF.Compilers;

namespace ContinuationParserCombinators
{
    public static class Parsers
    {
        internal static Result<TFuture> Best<TFuture>(Result<TFuture> result1, ForkableScanner scanner1, Result<TFuture> result2, ForkableScanner scanner2, ForkableScanner finalScanner)
        {
            if (result1 is Stop<TFuture>)
            {
                finalScanner.Join(scanner1);
                return result1;
            }
            if (result2 is Stop<TFuture>)
            {
                finalScanner.Join(scanner2);
                return result2;
            }

            var step1 = result1 as Step<TFuture>;
            var step2 = result2 as Step<TFuture>;

            if (step1.Cost < step2.Cost)
            {
                finalScanner.Join(scanner1);
                return step1;
            }
            else if (step1.Cost > step2.Cost)
            {
                finalScanner.Join(scanner2);
                return step2;
            }
            else
            {
                return Result.Step(1, () => Best(step1.NextResult, scanner1, step2.NextResult, scanner2, finalScanner));
            }
        }

        public static Parser<TResult> SelectMany<T1, T2, TResult>(this Parser<T1> parser, Func<T1, Parser<T2>> parserSelector, Func<T1, T2, TResult> resultSelector)
        {
            var parser2 = parserSelector(default(T1));

            return new ConcatenationSelect<T1, T2, TResult>(parser, parser2, resultSelector);
        }
    }
}
