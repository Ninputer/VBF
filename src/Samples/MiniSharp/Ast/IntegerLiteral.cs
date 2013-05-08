using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class IntegerLiteral : Expression
    {
        public LexemeValue Literal { get; private set; }
        public int Value { get; set; }

        public IntegerLiteral(LexemeValue literal)
        {
            Literal = literal;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitIntegerLiteral(this);
        }
    }
}
