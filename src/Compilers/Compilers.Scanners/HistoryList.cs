using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Scanners
{
    class HistoryList : IReadOnlyList<Lexeme>
    {
        private IReadOnlyList<Lexeme> m_fullHistory;
        private IReadOnlyList<int> m_valuableHistory;

        public HistoryList(IReadOnlyList<Lexeme> fullHistory, IReadOnlyList<int> valuableHistory)
        {
            m_fullHistory = fullHistory;
            m_valuableHistory = valuableHistory;
        }

        public Lexeme this[int index]
        {
            get { return m_fullHistory[m_valuableHistory[index]]; }
        }

        public int Count
        {
            get { return m_valuableHistory.Count; }
        }

        public IEnumerator<Lexeme> GetEnumerator()
        {
            for (int i = 0; i < m_valuableHistory.Count; i++)
            {
                yield return m_fullHistory[m_valuableHistory[i]];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
