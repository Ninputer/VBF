using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class BooleanType : Type
    {
        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitBooleanType(this);
        }
    }
}
