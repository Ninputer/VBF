using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class FiniteAutomationEngine
    {
        private int[][] m_acceptTables;
        private int[] m_currentAcceptTable;
        private int m_currentLexerState;

        private int[][] m_states;
        private int m_currentState;

        private ushort[] m_charClassTable;

        private int m_currentTokenIndex;


        public static FiniteAutomationEngine CreateFromLexicon(Lexicon lexicon)
        {
            throw new NotImplementedException();
        }
    }
}
