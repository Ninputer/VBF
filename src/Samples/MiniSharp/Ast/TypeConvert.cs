using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class TypeConvert : Expression
    {
        public Expression Source { get; private set; }

        public TypeConvert(Expression source, TypeBase targetType)
        {
            Source = source;
            ExpressionType = targetType;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitTypeConvert(this);
        }
    }
}
