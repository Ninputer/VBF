// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace VBF.Compilers.Scanners
{
    public sealed class LexemeValue
    {
        public LexemeValue(string content, SourceSpan span)
        {
            CodeContract.RequiresArgumentNotNull(span, "span");

            Content = content;
            Span = span;
        }

        public string Content { get; private set; }
        public SourceSpan Span { get; private set; }

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
