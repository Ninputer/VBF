using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    [Obsolete("Please use VBF.Compilers.Scanners.Scanner class instead")]
    public class PeekableScanner
    {
        private Scanner m_masterScanner;
        private CacheQueue<Lexeme> m_lookAheadQueue;

        public PeekableScanner(ScannerInfo scannerInfo)
        {
            CodeContract.RequiresArgumentNotNull(scannerInfo, "scannerInfo");

            m_lookAheadQueue = new CacheQueue<Lexeme>();
            m_masterScanner = new Scanner(scannerInfo);
        }

        public int Peek()
        {
            return Peek(1);
        }

        public int Peek2()
        {
            return Peek(2);
        }

        private Lexeme PeekLexeme(int lookAhead)
        {
            while (m_lookAheadQueue.Count < lookAhead)
            {
                //look ahead more
                m_lookAheadQueue.Enqueue(m_masterScanner.Read());
            }
            Lexeme lookAheadLexeme = m_lookAheadQueue[lookAhead - 1];
            return lookAheadLexeme;
        }

        public int Peek(int lookAhead)
        {
            CodeContract.RequiresArgumentInRange(lookAhead > 0, "lookAhead", "The lookAhead must be greater than zero");

            Lexeme lookAheadLexeme = PeekLexeme(lookAhead);
            return lookAheadLexeme.TokenIndex;

        }

        public int PeekInLexerState(int lexerStateIndex, int lookAhead)
        {
            CodeContract.RequiresArgumentInRange(lookAhead > 0, "lookAhead", "The lookAhead must be greater than zero");
            CodeContract.RequiresArgumentInRange(lexerStateIndex >= 0 && lexerStateIndex < m_masterScanner.ScannerInfo.LexerStateCount, "lexerStateIndex", "Invalid lexer state index");

            Lexeme lookAheadLexeme = PeekLexeme(lookAhead);
            return lookAheadLexeme.GetTokenIndex(lexerStateIndex);
        }

        public Lexeme Read()
        {
            if (m_lookAheadQueue.Count > 0)
            {
                return m_lookAheadQueue.Dequeue();
            }

            return m_masterScanner.Read();
        }

        public ScannerInfo ScannerInfo
        {
            get
            {
                return m_masterScanner.ScannerInfo;
            }
        }

        public void SetSource(SourceReader source)
        {
            if (m_lookAheadQueue.Count > 0)
            {
                throw new InvalidOperationException("The source is not allowed to be set when the look ahead queue not empty");
            }

            m_masterScanner.SetSource(source);
        }

        public void SetTriviaTokens(params int[] triviaTokenIndices)
        {
            if (m_lookAheadQueue.Count > 0)
            {
                throw new InvalidOperationException("The skip tokens are not allowed to be set when the look ahead queue not empty");
            }

            m_masterScanner.SetTriviaTokens(triviaTokenIndices);
        }

        public CompilationErrorList ErrorList
        {
            get
            {
                return m_masterScanner.ErrorList;
            }
            set
            {
                m_masterScanner.ErrorList = value;
            }
        }

        public bool RecoverErrors
        {
            get
            {
                return m_masterScanner.RecoverErrors;
            }
            set
            {
                m_masterScanner.RecoverErrors = value;
            }
        }

        public int LexicalErrorId
        {
            get
            {
                return m_masterScanner.LexicalErrorId;
            }
            set
            {
                m_masterScanner.LexicalErrorId = value;
            }
        }
    }
}
