using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.Compilers.Scanners.Generator
{
    public class DFAState
    {
        private List<DFAEdge> m_outEdges;
        private SortedSet<int> m_nfaStateSet;
        public int Index { get; internal set; }

        internal DFAState()
        {
            m_outEdges = new List<DFAEdge>();
            m_nfaStateSet = new SortedSet<int>();
        }

        public ReadOnlyCollection<DFAEdge> OutEdges
        {
            get
            {
                return m_outEdges.AsReadOnly();
            }
        }

        public ISet<int> NFAStateSet
        {
            get
            {
                return m_nfaStateSet;
            }
        }

        internal void AddEdge(DFAEdge edge)
        {
            CodeContract.RequiresArgumentNotNull(edge, "edge");

            m_outEdges.Add(edge);
        }


    }
}
