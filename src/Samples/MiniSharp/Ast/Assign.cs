using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class Assign : Statement
    {
        public VariableRef Variable { get; private set; }
        public Expression Value { get; private set; }

        public Assign(Lexeme varName, Expression value)
        {
            Variable = new VariableRef(varName);
            Value = value;
        }
    }
}
