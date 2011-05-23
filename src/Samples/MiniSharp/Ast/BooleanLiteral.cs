using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;
using VBF.Compilers;

namespace VBF.MiniSharp.Ast
{
    public class BooleanLiteral : Expression
    {
        public bool Value { get; private set; }
        private SourceSpan m_literalSpan;

        public BooleanLiteral(Lexeme literal)
        {
            Value = Boolean.Parse(literal.Value);
            m_literalSpan = literal.Span;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitBooleanLiteral(this);
        }
    }
}
