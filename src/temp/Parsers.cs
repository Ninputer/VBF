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
            throw new NotImplementedException();
        }
    }
}
