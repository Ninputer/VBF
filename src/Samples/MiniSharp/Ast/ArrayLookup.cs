using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class ArrayLookup : Expression
    {
        public Expression Array { get; set; }
        public Expression Index { get; private set; }
        public SourceSpan IndexSpan { get; private set; }

        public ArrayLookup(Expression array, Expression index, SourceSpan indexSpan)
        {
            Array = array;
            Index = index;
            IndexSpan = indexSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitArrayLookup(this);
        }
    }
}
