using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    public class FiniteAutomationEngine
    {
        private int[][] m_acceptTables;
        private int[] m_currentAcceptTable;
        private int m_currentLexerState;

        private int[][] m_transitionTable;
        private int m_currentState;

        private ushort[] m_charClassTable;

        private int m_currentTokenIndex;

        protected FiniteAutomationEngine(int[][] transitionTable, ushort[] charClassTable, int[][] acceptTables)
        {
            m_transitionTable = transitionTable;
            m_charClassTable = charClassTable;
            m_acceptTables = acceptTables;

            Debug.Assert(m_transitionTable.Length > 0);
            Debug.Assert(m_acceptTables.Length > 0);

            m_currentState = 1;
            m_currentLexerState = 0;
            m_currentAcceptTable = m_acceptTables[m_currentLexerState];
            m_currentTokenIndex = -1;
        }

        public int CurrentLexerStateIndex
        {
            get { return m_currentLexerState; }
            set
            {
                CodeContract.RequiresArgumentInRange(value >= 0 && value < m_acceptTables.Length, "value", 
                    "CurrentLexerState must be greater than or equal to 0 and less than the count of lexer states");

                m_currentLexerState = value;
                m_currentAcceptTable = m_acceptTables[m_currentLexerState];
            }
        }

        public void Reset()
        {
            m_currentState = 1;
            m_currentTokenIndex = -1;
        }

        public int CurrentTokenIndex
        {
            get
            {
                return m_currentTokenIndex;
            }
        }

        public bool IsAtAcceptState
        {
            get
            {
                return m_currentTokenIndex >= 0;
            }
        }

        public bool IsAtStoppedState
        {
            get
            {
                return m_currentState == 0;
            }
        }

        public void Input(char c)
        {
            int[] transitions = m_transitionTable[m_currentState];
            //find out which is the next state
            ushort charClass = m_charClassTable[c];
            int nextState = transitions[charClass];

            //go to next state
            m_currentState = nextState;
            m_currentTokenIndex = m_currentAcceptTable[nextState];
        }

        public void InputString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                Input(str[i]);
            }
        }

        public static FiniteAutomationEngine CreateFromLexicon(Lexicon lexicon)
        {
            CodeContract.RequiresArgumentNotNull(lexicon, "lexicon");

            DFAModel dfa = DFAModel.Create(lexicon);
            CompressedTransitionTable ctt = CompressedTransitionTable.Compress(dfa);

            FiniteAutomationEngine faEngine = new FiniteAutomationEngine(ctt.TransitionTable, ctt.CharClassTable, dfa.GetAcceptTables());

            return faEngine;
        }
    }
}
