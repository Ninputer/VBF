using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class VariableRef
    {
        public Lexeme VariableName { get; private set; }

        public VariableRef(Lexeme name)
        {
            VariableName = name;
        }
    }
}
