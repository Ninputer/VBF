using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class ArrayLength : Expression
    {
        public Expression Array { get; private set; }

        public ArrayLength(Expression array)
        {
            Array = array;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitArrayLength(this);
        }
    }
}
