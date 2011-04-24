using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class PeekableScanner
    {
        private Scanner m_masterScanner;
        private CacheQueue<Lexeme> m_lookAheadQueue;

        public PeekableScanner(Scanner masterScanner)
        {
            CodeContract.RequiresArgumentNotNull(masterScanner, "masterScanner");

            m_lookAheadQueue = new CacheQueue<Lexeme>();
            m_masterScanner = masterScanner;
        }

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

        public Scanner MasterScanner
        {
            get
            {
                return m_masterScanner;
            }
        }

        public void SetSource(SourceReader source)
        {
            m_masterScanner.SetSource(source);
        }

        public void SetSkipTokens(params int[] skipTokenIndices)
        {
            m_masterScanner.SetSkipTokens(skipTokenIndices);
        }
    }
}
