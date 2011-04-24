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

        internal Lexeme(ScannerInfo scannerInfo, int state, SourceSpan span, string value)
        {
            m_scannerInfo = scannerInfo;
            m_stateIndex = state;
            Span = span;
            Value = value;
        }

        public int TokenIndex
        {
            get
            {
                return m_scannerInfo.GetTokenIndex(m_stateIndex);
            }
        }

        internal int GetTokenIndex(int lexerState)
        {
            return m_scannerInfo.GetTokenIndex(m_stateIndex, lexerState);
        }
    }
}
