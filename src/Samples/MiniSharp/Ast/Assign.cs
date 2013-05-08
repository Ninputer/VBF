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
        public Expression Value { get; internal set; }

        public Assign(LexemeValue varName, Expression value)
        {
            Variable = new VariableRef(varName);
            Value = value;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitAssign(this);
        }
    }
}
