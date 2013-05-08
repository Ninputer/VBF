using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class VarDecl : Statement
    {
        public Type Type { get; private set; }
        public LexemeValue VariableName { get; private set; }

        public VarDecl(Type type, LexemeValue variableName)
        {
            Type = type;
            VariableName = variableName;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitVarDecl(this);
        }
    }
}
