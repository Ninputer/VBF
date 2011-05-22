using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class FieldDecl
    {
        public Type Type { get; private set; }
        public Lexeme FieldName { get; private set; }

        public FieldDecl(Type type, Lexeme fieldName)
        {
            Type = type;
            FieldName = fieldName;
        }
    }
}
