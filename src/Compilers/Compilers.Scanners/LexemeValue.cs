using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Scanners
{
    public sealed class LexemeValue
    {
        public string Content { get; private set; }
        public SourceSpan Span { get; private set; }

        internal LexemeValue(string content, SourceSpan span)
        {
            Content = content;
            Span = span;
        }

        public override string ToString()
        {
            return Content;
        }
    }
}
