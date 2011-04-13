using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VBF.Compilers.Scanners.Generator
{
    [DebuggerDisplay("State #{Index}")]
    public class NFAState
    {
        private List<NFAEdge> m_outEdges;
        public int Index { get; internal set; }
        internal int TokenIdentityIndex { get; set; }

        internal NFAState()
        {
            m_outEdges = new List<NFAEdge>();

            //default value -1 means no token is bound with this state
            TokenIdentityIndex = -1;
        }

        public ReadOnlyCollection<NFAEdge> OutEdges
        {
            get
            {
                return m_outEdges.AsReadOnly();
            }
        }

        internal void AddEmptyEdgeTo(NFAState targetState)
        {
            CodeContract.RequiresArgumentNotNull(targetState, "targetState");

            m_outEdges.Add(new NFAEdge(targetState));
        }

        internal void AddEdge(NFAEdge edge)
        {
            CodeContract.RequiresArgumentNotNull(edge, "edge");

            m_outEdges.Add(edge);
        }


    }
}
