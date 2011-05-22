using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class BooleanLiteral : Expression
    {
        public bool Value { get; private set; }
        public BooleanLiteral(string literal)
        {
            Value = Boolean.Parse(literal);
        }
    }
}
