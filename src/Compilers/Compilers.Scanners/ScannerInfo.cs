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

namespace VBF.Compilers.Scanners
{
    public class ScannerInfo
    {

        private SerializableScannerInfo m_detail;
        private int m_lexerState;

        internal ScannerInfo(int[][] transitionTable, ushort[] charClassTable, int[][] acceptTables, int tokenCount)
        {
            m_detail = new SerializableScannerInfo(transitionTable, charClassTable, acceptTables, tokenCount);
            EndOfStreamTokenIndex = tokenCount;
        }

        public int EndOfStreamTokenIndex { get; private set; }

        internal int[][] TransitionTable
        {
            get { return m_detail.TransitionTable; }
        }

        internal ushort[] CharClassTable
        {
            get { return m_detail.CharClassTable; }
        }

        internal int TokenCount
        {
            get { return m_detail.TokenCount; }
        }

        internal int EndOfStreamState
        {
            get { return m_detail.TransitionTable.Length; }
        }

        public int LexerStateCount
        {
            get { return m_detail.AcceptTables.Length; }
        }

        public int CurrentLexerIndex
        {
            get
            {
                return m_lexerState;
            }
            set
            {
                CodeContract.RequiresArgumentInRange(value >= 0 && value < m_detail.AcceptTables.Length, "value", "Invalid lexer index");
                m_lexerState = value;
            }
        }

        internal int GetStateIndex(int tokenIndex)
        {
            int possibleIndex = -1;
            for (int i = 0; i < m_detail.AcceptTables.Length; i++)
            {
                possibleIndex = Array.IndexOf(m_detail.AcceptTables[i], tokenIndex);

                if (possibleIndex >= 0)
                {
                    break;
                }
            }
            return possibleIndex;
        }

        internal int GetStateIndex(int tokenIndex, int state)
        {
            return Array.IndexOf(m_detail.AcceptTables[state], tokenIndex);
        }

        internal int GetTokenIndex(int state)
        {
            return m_detail.AcceptTables[m_lexerState][state];
        }

        internal int GetTokenIndex(int state, int lexerState)
        {
            return m_detail.AcceptTables[lexerState][state];
        }
    }

    struct SerializableScannerInfo
    {
        internal int[][] AcceptTables;
        internal ushort[] CharClassTable;
        internal int TokenCount;
        internal int[][] TransitionTable;

        internal SerializableScannerInfo(int[][] transitionTable, ushort[] charClassTable, int[][] acceptTables, int tokenCount)
        {
            TransitionTable = transitionTable;
            CharClassTable = charClassTable;
            AcceptTables = acceptTables;
            TokenCount = tokenCount;
        }
    }
}
