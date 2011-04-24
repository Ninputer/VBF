using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class ScannerInfo
    {

        private SerializableScannerInfo m_detail;
        private int m_lexerState;
        public int EndOfStreamTokenIndex { get; private set; }

        internal ScannerInfo(int[][] transitionTable, ushort[] charClassTable, int[][] acceptTables, int tokenCount)
        {
            m_detail = new SerializableScannerInfo(transitionTable, charClassTable, acceptTables, tokenCount);
            EndOfStreamTokenIndex = tokenCount;
        }

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

        internal int GetTokenIndex(int state)
        {
            return m_detail.AcceptTables[m_lexerState][state];
        }

        internal int GetTokenIndex(int state, int lexerState)
        {
            return m_detail.AcceptTables[lexerState][state];
        }

        internal int EndOfStreamState
        {
            get { return m_detail.TransitionTable.Length; }
        }

        public int LexerStateCount
        {
            get { return m_detail.AcceptTables.Length; }
        }

        public int LexerStateIndex
        {
            get
            {
                return m_lexerState;
            }
            set
            {
                CodeContract.RequiresArgumentInRange(value >= 0 && value < m_detail.AcceptTables.Length, "value", "Invalid lexer state index");
                m_lexerState = value;
            }
        }
    }

    struct SerializableScannerInfo
    {
        internal int[][] TransitionTable;
        internal ushort[] CharClassTable;
        internal int[][] AcceptTables;
        internal int TokenCount;

        internal SerializableScannerInfo(int[][] transitionTable, ushort[] charClassTable, int[][] acceptTables, int tokenCount)
        {
            TransitionTable = transitionTable;
            CharClassTable = charClassTable;
            AcceptTables = acceptTables;
            TokenCount = tokenCount;
        }
    }
}
