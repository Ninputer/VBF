using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class TypeRef
    {
        public Lexeme TypeName { get; private set; }
        public TypeBase Type { get; set; }
        
        public TypeRef(Lexeme name)
        {
            TypeName = name;
        }
    }
}
