using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public interface IProduction
    {
        TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument);
        bool IsTerminal { get; }
        bool IsEos { get; }
        bool AggregatesAmbiguities { get; }
    }
}
