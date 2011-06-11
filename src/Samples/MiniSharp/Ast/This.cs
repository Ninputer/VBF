using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class This : Expression
    {
        public SourceSpan ThisSpan { get; private set; }
        public This(SourceSpan thisSpan)
        {
            ThisSpan = thisSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitThis(this);
        }
    }
}
