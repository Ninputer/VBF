using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class NewArray : Expression
    {
        public Expression Length { get; private set; }
        public SourceSpan LengthSpan { get; private set; }

        public NewArray(Expression length, SourceSpan lengthSpan)
        {
            Length = length;
            LengthSpan = lengthSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitNewArray(this);
        }
    }
}
