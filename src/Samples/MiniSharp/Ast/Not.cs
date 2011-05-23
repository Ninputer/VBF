using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class Not : Expression
    {
        public Expression Operand { get; private set; }

        public Not(Expression exp)
        {
            Operand = exp;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitNot(this);
        }
    }
}
