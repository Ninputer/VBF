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
using System.Collections.Generic;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    [DebuggerDisplay("Token:{TokenIndex} {Value.ToString()}")]
    public sealed class Lexeme
    {
        private static readonly Lexeme[] s_emptyTrivia = new Lexeme[0];
        private ScannerInfo m_scannerInfo;
        private int m_stateIndex;
        private IReadOnlyList<Lexeme> m_trivia;

        internal Lexeme(ScannerInfo scannerInfo, int state, SourceSpan span, string content)
        {
            m_scannerInfo = scannerInfo;
            m_stateIndex = state;
            Value = new LexemeValue(content, span);

            m_trivia = s_emptyTrivia;
        }

        public LexemeValue Value { get; private set; }

        public int TokenIndex
        {
            get
            {
                return m_scannerInfo.GetTokenIndex(m_stateIndex);
            }
        }

        public IReadOnlyList<Lexeme> PrefixTrivia
        {
            get
            {
                return m_trivia;
            }
        }

        public bool IsEndOfStream
        {
            get
            {
                return m_stateIndex == m_scannerInfo.EndOfStreamState;
            }
        }

        internal void SetTrivia(IReadOnlyList<Lexeme> trivia)
        {
            m_trivia = trivia;
        }

        public int GetTokenIndex(int lexerState)
        {
            return m_scannerInfo.GetTokenIndex(m_stateIndex, lexerState);
        }

        public Lexeme GetErrorCorrectionLexeme(int expectedTokenIndex, string expectedValue)
        {
            int state = m_scannerInfo.GetStateIndex(expectedTokenIndex);
            if (state < 0) throw new ArgumentException("Expected token index is invalid", "expectedTokenIndex");

            return new Lexeme(m_scannerInfo, state, new SourceSpan(Value.Span.StartLocation, Value.Span.StartLocation), expectedValue);
        }
    }
}
