using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class VariableRef
    {
        public LexemeValue VariableName { get; private set; }
        public VariableInfo VariableInfo { get; set; }

        public VariableRef(LexemeValue name)
        {
            VariableName = name;
        }
    }
}
