using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    class FiniteAutomationEngine
    {

        private int[][] m_transitionTable;
        private int m_currentState;

        private ushort[] m_charClassTable;

        public FiniteAutomationEngine(int[][] transitionTable, ushort[] charClassTable)
        {
            m_transitionTable = transitionTable;
            m_charClassTable = charClassTable;

            Debug.Assert(m_transitionTable.Length > 0);

            m_currentState = 1;
        }

        public void Reset()
        {
            m_currentState = 1;
        }
        public int CurrentState
        {
            get
            {
                return m_currentState;
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
        }

        public void InputString(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                Input(str[i]);
            }
        }
    }
}
