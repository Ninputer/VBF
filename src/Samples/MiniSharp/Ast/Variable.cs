using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class Variable : Expression
    {
        public VariableRef VariableRef { get; private set; }

        public Variable(Lexeme name)
        {
            VariableRef = new VariableRef(name);
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitVariable(this);
        }
    }
}
