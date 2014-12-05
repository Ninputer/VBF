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
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class Scanner
    {
        //consts
        private const int c_skip = 1;

        //members

        private FiniteAutomationEngine m_engine;
        //private List<Lexeme> m_triviaCache;
        private List<Lexeme> m_fullHistory;
        private HistoryList m_historyList;

        private int m_lastNotSkippedLexemeIndex;
        private int m_lastState;
        private SourceLocation m_lastTokenStart;
        private StringBuilder m_lexemeValueBuilder;
        private ScannerInfo m_scannerInfo;
        private SourceReader m_source;

        private int[] m_tokenAttributes;
        private int m_valuableCursor;
        private List<int> m_valuableHistory;

        public Scanner(ScannerInfo scannerInfo)
        {
            m_scannerInfo = scannerInfo;

            m_engine = new FiniteAutomationEngine(m_scannerInfo.TransitionTable, m_scannerInfo.CharClassTable);
            m_lexemeValueBuilder = new StringBuilder(32);
            m_tokenAttributes = new int[scannerInfo.TokenCount];

            Initialize();
        }

        public CompilationErrorList ErrorList { get; set; }
        public bool RecoverErrors { get; set; }
        public int LexicalErrorId { get; set; }

        public ScannerInfo ScannerInfo
        {
            get { return m_scannerInfo; }
        }

        public int ReadingIndex
        {
            get
            {
                return m_valuableCursor;
            }
        }        

        public IReadOnlyList<Lexeme> History
        {
            get
            {
                return m_historyList;
            }
        }

        private void Initialize()
        {
            m_engine.Reset();
            m_lastState = 0;
            m_lexemeValueBuilder.Clear();

            m_fullHistory = new List<Lexeme>();
            m_valuableHistory = new List<int>();
            m_historyList = new HistoryList(m_fullHistory, m_valuableHistory);

            m_valuableCursor = 0;
            m_lastNotSkippedLexemeIndex = 0;
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
                    m_tokenAttributes[skipIndex] = c_skip;
                }
            }
        }

        public Lexeme Read()
        {
            if (m_valuableCursor < m_valuableHistory.Count)
            {
                int fullCursor = m_valuableHistory[m_valuableCursor++];

                return m_fullHistory[fullCursor];
            }

            //m_triviaCache.Clear();

            while (true)
            {
                //run to next stopped state
                m_engine.Reset();
                m_lastTokenStart = m_source.PeekLocation();
                m_lastState = 0;

                m_lexemeValueBuilder.Clear();

                if (m_source.PeekChar() < 0)
                {
                    //return End Of Stream token
                    AddHistory(new Lexeme(m_scannerInfo, m_scannerInfo.EndOfStreamState,
                        new SourceSpan(m_lastTokenStart, m_lastTokenStart), null));

                    break;
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
                    m_lastState = m_engine.CurrentState;

                    m_source.ReadChar();
                    m_lexemeValueBuilder.Append(inputChar);
                }

                //skip tokens that marked with "Skip" attribute
                bool isLastSkipped = IsLastTokenSkippable();

                if (isLastSkipped)
                {
                    AddHistory(new Lexeme(m_scannerInfo, m_lastState,
                        new SourceSpan(m_lastTokenStart, m_source.Location), m_lexemeValueBuilder.ToString()), false);
                }
                else
                {
                    AddHistory(new Lexeme(m_scannerInfo, m_lastState,
                        new SourceSpan(m_lastTokenStart, m_source.Location), m_lexemeValueBuilder.ToString()));


                    break;
                }
            }

            int lastTokenfullCursor = m_valuableHistory[m_valuableCursor - 1];
            return m_fullHistory[lastTokenfullCursor];
        }

        public void Seek(int index)
        {
            CodeContract.RequiresArgumentInRange(index >= 0, "index", "Seek index must be greater than or equal to 0");

            if (index <= m_valuableHistory.Count)
            {
                m_valuableCursor = index;
            }
            else
            {
                //index > m_history.Count
                int restCount = index - m_valuableHistory.Count;

                m_valuableCursor = m_valuableHistory.Count;
                for (int i = 0; i < restCount; i++)
                {
                    Read();
                }
            }
        }

        public int Peek(int lookAhead)
        {
            CodeContract.RequiresArgumentInRange(lookAhead > 0, "lookAhead", "The lookAhead must be greater than zero");

            Lexeme lookAheadLexeme = PeekLexeme(lookAhead);
            return lookAheadLexeme.TokenIndex;

        }

        public int Peek()
        {
            return Peek(1);
        }

        public int Peek2()
        {
            return Peek(2);
        }

        public int PeekInLexerState(int lexerStateIndex, int lookAhead)
        {
            CodeContract.RequiresArgumentInRange(lookAhead > 0, "lookAhead", "The lookAhead must be greater than zero");
            CodeContract.RequiresArgumentInRange(lexerStateIndex >= 0 && lexerStateIndex < ScannerInfo.LexerStateCount, "lexerStateIndex", "Invalid lexer state index");

            Lexeme lookAheadLexeme = PeekLexeme(lookAhead);
            return lookAheadLexeme.GetTokenIndex(lexerStateIndex);
        }

        private void AddHistory(Lexeme lexeme, bool setTrivia = true)
        {
            Debug.Assert(m_valuableCursor == m_valuableHistory.Count);

            m_fullHistory.Add(lexeme);
            int fullCursor = m_fullHistory.Count();

            if (setTrivia)
            {
                int lastTriviaStartIndex = m_lastNotSkippedLexemeIndex + 1;
                int lastTriviaLength = fullCursor - 1 - lastTriviaStartIndex;

                if (lastTriviaLength < 0)
                {
                    lastTriviaLength = 0;
                }

                lexeme.SetTrivia(new LexemeRange(m_fullHistory, lastTriviaStartIndex, lastTriviaLength));
                m_lastNotSkippedLexemeIndex = fullCursor - 1;

                m_valuableHistory.Add(fullCursor - 1);
                m_valuableCursor = m_valuableHistory.Count;
            }

        }

        private bool IsLastTokenSkippable()
        {
            int acceptTokenIndex = m_scannerInfo.GetTokenIndex(m_lastState);

            if (acceptTokenIndex < 0 && RecoverErrors)
            {
                //eat one char to continue
                m_lexemeValueBuilder.Append((char)m_source.ReadChar());

                if (ErrorList != null)
                {
                    ErrorList.AddError(LexicalErrorId, new SourceSpan(m_lastTokenStart, m_source.Location), m_lexemeValueBuilder.ToString());
                }

                return true;
            }

            return acceptTokenIndex >= 0 && m_tokenAttributes[acceptTokenIndex] == c_skip;
        }

        private Lexeme PeekLexeme(int lookAhead)
        {
            int currentCursor = m_valuableCursor;

            m_valuableCursor = m_valuableHistory.Count;
            while (currentCursor + lookAhead - 1 >= m_valuableHistory.Count)
            {
                //look ahead more
                Read();
            }
            int lookAheadFullIndex = m_valuableHistory[currentCursor + lookAhead - 1];
            Lexeme lookAheadLexeme = m_fullHistory[lookAheadFullIndex];

            m_valuableCursor = currentCursor;

            return lookAheadLexeme;
        }     
    }
}
