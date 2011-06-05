using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class Not : Expression
    {
        public Expression Operand { get; private set; }
        public SourceSpan OpSpan { get; private set; }

        public Not(Expression exp, SourceSpan opSpan)
        {
            Operand = exp;
            OpSpan = opSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitNot(this);
        }
    }
}
