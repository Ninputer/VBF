using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    public class Scanner
    {
        //consts
        private const int Skip = 1;

        //members
        private ScannerInfo m_scannerInfo;

        private FiniteAutomationEngine m_engine;
        private SourceReader m_source;

        private int m_lastState;
        private SourceLocation m_lastTokenStart;
        private StringBuilder m_lexemeValueBuilder;
        private List<Lexeme> m_triviaCache;

        private int[] m_tokenAttributes;

        public CompilationErrorManager ErrorManager { get; set; }
        public bool RecoverErrors { get; set; }
        public int LexicalErrorId { get; set; }

        public Scanner(ScannerInfo scannerInfo)
        {
            m_scannerInfo = scannerInfo;

            m_engine = new FiniteAutomationEngine(m_scannerInfo.TransitionTable, m_scannerInfo.CharClassTable);
            m_lexemeValueBuilder = new StringBuilder(32);
            m_tokenAttributes = new int[scannerInfo.TokenCount];

            m_triviaCache = new List<Lexeme>();

            Initialize();
        }

        private void Initialize()
        {
            m_engine.Reset();
            m_lastState = 0;
            m_lexemeValueBuilder.Clear();
        }

        public void SetSource(SourceReader source)
        {
            CodeContract.RequiresArgumentNotNull(source, "source");
            m_source = source;

            Initialize();
        }

        public void SetTriviaTokens(params int[] triviaTokenIndices)
        {
            Array.Clear(m_tokenAttributes, 0, m_tokenAttributes.Length);

            for (int i = 0; i < triviaTokenIndices.Length; i++)
            {
                int skipIndex = triviaTokenIndices[i];

                if (skipIndex >= 0 && skipIndex < m_tokenAttributes.Length)
                {
                    m_tokenAttributes[skipIndex] = Skip;
                }
            }
        }

        public ScannerInfo ScannerInfo
        {
            get { return m_scannerInfo; }
        }

        public Lexeme Read()
        {
            m_triviaCache.Clear();
            bool isLastSkipped = false;

            do
            {
                //run to next stopped state
                m_engine.Reset();
                m_lastTokenStart = m_source.PeekLocation();
                m_lastState = 0;

                m_lexemeValueBuilder.Clear();

                if (m_source.PeekChar() < 0)
                {
                    //return End Of Stream token
                    return new Lexeme(m_scannerInfo, m_scannerInfo.EndOfStreamState,
                        new SourceSpan(m_lastTokenStart, m_lastTokenStart), null, m_triviaCache);
                }

                while (true)
                {
                    int inputCharValue = m_source.PeekChar();

                    if (inputCharValue < 0)
                    {
                        //end of stream, treat as stopped
                        break;
                    }

                    char inputChar = (char)inputCharValue;
                    m_engine.Input(inputChar);

                    if (m_engine.IsAtStoppedState)
                    {
                        //stop immediately at a stopped state
                        break;
                    }
                    else
                    {
                        m_lastState = m_engine.CurrentState;
                    }

                    m_source.ReadChar();
                    m_lexemeValueBuilder.Append(inputChar);
                }

                //skip tokens that marked with "Skip" attribute
                isLastSkipped = IsLastTokenSkippable();

                if (isLastSkipped)
                {
                    m_triviaCache.Add(new Lexeme(m_scannerInfo, m_lastState,
                        new SourceSpan(m_lastTokenStart, m_source.Location), m_lexemeValueBuilder.ToString(), null));
                }

            } while (isLastSkipped);

            return new Lexeme(m_scannerInfo, m_lastState,
                new SourceSpan(m_lastTokenStart, m_source.Location), m_lexemeValueBuilder.ToString(), m_triviaCache);
        }

        private bool IsLastTokenSkippable()
        {
            int acceptTokenIndex = m_scannerInfo.GetTokenIndex(m_lastState);

            if (acceptTokenIndex < 0 && RecoverErrors)
            {
                //eat one char to continue
                m_lexemeValueBuilder.Append((char)m_source.ReadChar());

                if (ErrorManager != null)
                {
                    ErrorManager.AddError(LexicalErrorId, new SourceSpan(m_lastTokenStart, m_source.Location), m_lexemeValueBuilder.ToString());
                }

                return true;
            }

            return acceptTokenIndex >= 0 && m_tokenAttributes[acceptTokenIndex] == Skip;
        }
    }
}
