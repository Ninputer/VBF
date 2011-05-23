using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class ArrayAssign : Statement
    {
        public VariableRef Array{ get; private set; }
        public Expression Index { get; private set; }
        public Expression Value { get; private set; }

        public ArrayAssign(Lexeme arrayName, Expression index, Expression value)
        {
            Array = new VariableRef(arrayName);
            Index = index;
            Value = value;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitArrayAssign(this);
        }
    }
}
