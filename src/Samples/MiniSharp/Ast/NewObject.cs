using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class NewObject : Expression
    {
        public TypeRef Type { get; private set; }

        public NewObject(LexemeValue typeName)
        {
            Type = new TypeRef(typeName);
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitNewObject(this);
        }
    }
}
