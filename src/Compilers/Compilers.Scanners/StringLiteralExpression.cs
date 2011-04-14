using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public sealed class StringLiteralExpression : RegularExpression
    {
        public new string Literal { get; private set; }

        public StringLiteralExpression(string literal)
            : base(RegularExpressionType.StringLiteral)
        {
            Literal = literal;
        }

        public override string ToString()
        {
            return Literal;
        }
    }
}
