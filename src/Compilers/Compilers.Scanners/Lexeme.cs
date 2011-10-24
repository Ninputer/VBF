using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace VBF.Compilers.Scanners
{
    [DebuggerDisplay("Token:{TokenIndex} {Value}")]
    public sealed class Lexeme
    {
        private ScannerInfo m_scannerInfo;
        private int m_stateIndex;
        private readonly Lexeme[] m_triviaArray;
        private readonly ReadOnlyCollection<Lexeme> m_trivia;

        private static readonly Lexeme[] s_emptyTrivia = new Lexeme[0];

        public SourceSpan Span { get; private set; }
        public string Value { get; private set; }

        internal Lexeme(ScannerInfo scannerInfo, int state, SourceSpan span, string value, List<Lexeme> trivia)
        {
            m_scannerInfo = scannerInfo;
            m_stateIndex = state;
            Span = span;
            Value = value;

            if (trivia != null)
            {
                m_triviaArray = trivia.ToArray();
                m_trivia = new ReadOnlyCollection<Lexeme>(m_triviaArray);

            }
            else
            {
                m_triviaArray = null;
                m_trivia = new ReadOnlyCollection<Lexeme>(s_emptyTrivia);
            }

            
        }

        public int TokenIndex
        {
            get
            {
                return m_scannerInfo.GetTokenIndex(m_stateIndex);
            }
        }

        public ReadOnlyCollection<Lexeme> Trivia
        {
            get
            {
                return m_trivia;
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

            return new Lexeme(m_scannerInfo, state, new SourceSpan(Span.StartLocation, Span.StartLocation), expectedValue, null);
        }
    }
}
