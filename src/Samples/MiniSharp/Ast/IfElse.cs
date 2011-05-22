using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class IfElse : Statement
    {
        public Expression Condition { get; private set; }
        public Statement TruePart { get; private set; }
        public Statement FalsePart { get; private set; }

        public IfElse(Expression cond, Statement truePart, Statement falsePart)
        {
            Condition = cond;
            TruePart = truePart;
            FalsePart = falsePart;
        }
    }
}
