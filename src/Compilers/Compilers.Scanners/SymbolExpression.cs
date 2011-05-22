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
        public new char Symbol { get; private set; }

        public SymbolExpression(char symbol)
            : base(RegularExpressionType.Symbol)
        {
            Symbol = symbol;
        }

        public override string ToString()
        {
            return Symbol.ToString();
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return new Func<HashSet<char>>[0];
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            var result = new HashSet<char>();
            result.Add(Symbol);

            return result;
        }

        internal override T Accept<T>(RegularExpressionConverter<T> converter)
        {
            return converter.ConvertSymbol(this);
        }
    }
}
