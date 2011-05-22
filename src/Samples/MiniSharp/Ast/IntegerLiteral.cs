using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class IntegerLiteral : Expression
    {
        public string Literal { get; private set; }

        public IntegerLiteral(string literal)
        {
            Literal = literal;
        }
    }
}
