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
        private ScannerInfo m_scannerInfo;
        private int m_stateIndex;

        public SourceSpan Span { get; private set; }
        public string Value { get; private set; }
        public int SkippedTokenCount { get; private set; }

        internal Lexeme(ScannerInfo scannerInfo, int state, SourceSpan span, string value, int skippedTokenCount)
        {
            m_scannerInfo = scannerInfo;
            m_stateIndex = state;
            Span = span;
            Value = value;
            SkippedTokenCount = skippedTokenCount;
        }

        public int TokenIndex
        {
            get
            {
                return m_scannerInfo.GetTokenIndex(m_stateIndex);
            }
        }

        public int GetTokenIndex(int lexerState)
        {
            return m_scannerInfo.GetTokenIndex(m_stateIndex, lexerState);
        }

        public bool IsEndOfStream
        {
            get
            {
                return m_stateIndex == m_scannerInfo.EndOfStreamState;
            }
        }

        public Lexeme GetErrorCorrectionLexeme(int expectedTokenIndex, string expectedValue)
        {
            int state = m_scannerInfo.GetStateIndex(expectedTokenIndex);
            if (state < 0) throw new ArgumentException("Expected token index is invalid", "expectedTokenIndex");

            return new Lexeme(m_scannerInfo, state, new SourceSpan(Span.StartLocation, Span.StartLocation), expectedValue, 0);
        }
    }
}
