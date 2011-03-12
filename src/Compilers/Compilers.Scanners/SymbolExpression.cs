using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// Represents a regular expression accepts a literal character
    /// </summary>
    public class SymbolExpression : RegularExpression
    {
        public char Symbol { get; private set; }

        public SymbolExpression(char symbol)
            : base(RegularExpressionType.Symbol)
        {
            Symbol = symbol;
        }
    }
}
