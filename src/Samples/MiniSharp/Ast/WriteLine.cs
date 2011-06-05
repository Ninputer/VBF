using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class WriteLine : Statement
    {
        public Expression Value { get; private set; }
        public SourceSpan WriteLineSpan { get; private set; }

        public WriteLine(Expression value, SourceSpan writelineSpan)
        {
            Value = value;
            WriteLineSpan = writelineSpan;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitWriteLine(this);
        }
    }
}
