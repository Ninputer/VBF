using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.Generator
{
    public struct LR0Edge : IEquatable<LR0Edge>
    {
        private int m_symbolIndex;
        private int m_sourceStateIndex;
        private int m_targetStateIndex;

        internal LR0Edge(int sourceStateIndex, int symbolIndex, int targetStateIndex)
        {
            m_sourceStateIndex = sourceStateIndex;
            m_symbolIndex = symbolIndex;
            m_targetStateIndex = targetStateIndex;
        }

        public bool Equals(LR0Edge other)
        {
            return m_symbolIndex == other.m_symbolIndex && 
                m_targetStateIndex == other.m_targetStateIndex &&
                m_sourceStateIndex == other.m_sourceStateIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            LR0Edge other = (LR0Edge)obj;

            return Equals(other); 
        }

        public override int GetHashCode()
        {
            return (m_sourceStateIndex << 24) ^ (m_symbolIndex << 12) ^ m_targetStateIndex;
        }
    }
}
