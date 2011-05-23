using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class While : Statement
    {
        public Expression Condition { get; private set; }
        public Statement LoopBody { get; private set; }

        public While(Expression cond, Statement body)
        {
            Condition = cond;
            LoopBody = body;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitWhile(this);
        }
    }
}
