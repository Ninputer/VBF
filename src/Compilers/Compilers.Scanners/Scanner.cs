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

        private int[] m_tokenAttributes;

        public Scanner(ScannerInfo scannerInfo)
        {
            m_scannerInfo = scannerInfo;

            m_engine = new FiniteAutomationEngine(m_scannerInfo.TransitionTable, m_scannerInfo.CharClassTable);
            m_lexemeValueBuilder = new StringBuilder();
            m_tokenAttributes = new int[scannerInfo.TokenCount];

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

        public void SetSkipTokens(params int[] skipTokenIndices)
        {
            Array.Clear(m_tokenAttributes, 0, m_tokenAttributes.Length);

            for (int i = 0; i < skipTokenIndices.Length; i++)
            {
                int skipIndex = skipTokenIndices[i];

                if (skipIndex >= 0 && skipIndex < m_tokenAttributes.Length)
                {
                    m_tokenAttributes[skipIndex] = Skip;
                }
            }
        }     

        public Lexeme Read()
        {
            return ReadNextToken();
        }

        public ScannerInfo ScannerInfo
        {
            get { return m_scannerInfo; }
        }

        private Lexeme ReadNextToken()
        {
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
                        new SourceSpan(m_lastTokenStart, m_lastTokenStart), null);
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
            } while (IsLastTokenSkippable());

            return new Lexeme(m_scannerInfo, m_lastState,
                new SourceSpan(m_lastTokenStart, m_source.Location), m_lexemeValueBuilder.ToString());
        }

        private bool IsLastTokenSkippable()
        {
            int acceptTokenIndex = m_scannerInfo.GetTokenIndex(m_lastState);
            return acceptTokenIndex >= 0 && m_tokenAttributes[acceptTokenIndex] == Skip;
        }
    }
}
