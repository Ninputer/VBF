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

        public LexemeValue(string content, SourceSpan span)
        {
            CodeContract.RequiresArgumentNotNull(span, "span");

            Content = content;
            Span = span;
        }

        public override string ToString()
        {
            return Content;
        }
    }

    public static class LexemeExtensions
    {
        /// <summary>
        /// Gets the Value of a lexeme if it is not null, otherwise null(Nothing in Visual Basic).
        /// </summary>
        /// <param name="lexeme">The lexeme that the value is going to get from.</param>
        /// <returns>Value of a lexeme if it is not null, otherwise null(Nothing in Visual Basic).</returns>
        public static LexemeValue GetValue(this Lexeme lexeme)
        {
            if (lexeme != null)
            {
                return lexeme.Value;
            }
            else
            {
                return null;
            }
        }
    }
}
