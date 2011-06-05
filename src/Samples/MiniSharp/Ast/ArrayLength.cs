using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class ArrayLength : Expression
    {
        public Expression Array { get; private set; }
        public SourceSpan LengthSpan { get; private set; }

        public ArrayLength(Expression array, SourceSpan lengthSpan)
        {
            Array = array;
            LengthSpan = lengthSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitArrayLength(this);
        }
    }
}
