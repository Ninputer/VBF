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

using System;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class TokenParser : Parser<Lexeme>
    {
        private Func<Lexeme, bool> m_qualificationPredicate;

        public TokenParser(Token expected, int? lexerState, Func<Lexeme, bool> qualificationPredicate, string missingCorrection)
        {
            CodeContract.RequiresArgumentNotNull(expected, "expected");
            CodeContract.RequiresArgumentNotNull(qualificationPredicate, "qualificationPredicate");

            ExpectedToken = expected;
            MissingCorrection = missingCorrection;
            m_qualificationPredicate = qualificationPredicate;
            LexerStateIndex = lexerState;
        }

        public TokenParser(Token expected, int? lexerState, Func<Lexeme, bool> qualificationPredicate)
        {
            CodeContract.RequiresArgumentNotNull(expected, "expected");
            CodeContract.RequiresArgumentNotNull(qualificationPredicate, "qualificationPredicate");

            ExpectedToken = expected;
            MissingCorrection = expected.ToString();
            m_qualificationPredicate = qualificationPredicate;
            LexerStateIndex = lexerState;
        }

        public TokenParser(Token expected, int? lexerState) : this(expected, lexerState, l => true) { }
        public Token ExpectedToken { get; private set; }
        public string MissingCorrection { get; private set; }
        public int? LexerStateIndex { get; private set; }

        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<Lexeme, TFuture> future)
        {
            ParserFunc<TFuture> scan = null;
            scan = (scanner, context) =>
            {
                var s1 = scanner.Fork();

                var l = scanner.Read();

                int tokenIndex;
                if (LexerStateIndex.HasValue)
                {
                    tokenIndex = l.GetTokenIndex(LexerStateIndex.Value);
                }
                else
                {
                    tokenIndex = l.TokenIndex;
                }

                if (tokenIndex == ExpectedToken.Index && m_qualificationPredicate(l))
                {
                    var r = context.StepResult(0, () => future(l)(scanner, context));
                    return r;
                }
                Lexeme correctionLexeme = l.GetErrorCorrectionLexeme(ExpectedToken.Index, MissingCorrection);
                ErrorCorrection insertCorrection = new InsertedErrorCorrection(ExpectedToken.ToString(), correctionLexeme.Value.Span);

                if (l.IsEndOfStream)
                {
                    scanner.Join(s1);
                    return context.StepResult(1, () => future(correctionLexeme)(scanner, context), insertCorrection); //insert
                }
                else
                {
                    ErrorCorrection deleteCorrection = new DeletedErrorCorrection(l);
                    return context.ChooseBest(context.StepResult(1, () => future(correctionLexeme)(s1, context), insertCorrection), //insert
                        context.StepResult(1, () => scan(scanner, context), deleteCorrection)); //delete
                }
            };

            return scan;
        }
    }
}
