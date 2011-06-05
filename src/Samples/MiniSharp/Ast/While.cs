using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class While : Statement
    {
        public SourceSpan WhileSpan { get; private set; }
        public Expression Condition { get; private set; }
        public Statement LoopBody { get; private set; }

        public While(Expression cond, Statement body, SourceSpan whileSpan)
        {
            Condition = cond;
            LoopBody = body;
            WhileSpan = whileSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }
    }
}
