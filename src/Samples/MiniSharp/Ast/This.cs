using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class This : Expression
    {
        public This()
        {

        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitThis(this);
        }
    }
}
