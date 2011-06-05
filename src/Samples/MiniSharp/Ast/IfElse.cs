using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class IfElse : Statement
    {
        public Expression Condition { get; private set; }
        public Statement TruePart { get; private set; }
        public Statement FalsePart { get; private set; }
        public SourceSpan IfSpan { get; private set; }
        public SourceSpan ElseSpan { get; private set; }

        public IfElse(Expression cond, Statement truePart, Statement falsePart, SourceSpan ifSpan, SourceSpan elseSpan)
        {
            Condition = cond;
            TruePart = truePart;
            FalsePart = falsePart;
            IfSpan = ifSpan;
            ElseSpan = elseSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitIfElse(this);
        }
    }
}
