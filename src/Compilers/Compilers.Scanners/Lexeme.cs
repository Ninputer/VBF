using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    [DebuggerDisplay("Token:{TokenIndex} {Value}")]
    public sealed class Lexeme
    {
        public int TokenIndex { get; private set; }
        public SourceSpan Span { get; private set; }
        public string Value { get; private set; }

        internal Lexeme(int tokenIndex, SourceSpan span, string value)
        {
            TokenIndex = tokenIndex;
            Span = span;
            Value = value;
        }
    }
}
