using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    class ForkNode
    {
        private Scanner m_masterScanner;
        private CacheQueue<Lexeme> m_lookAheadQueue;

        private ForkNode m_parent;
        private List<ForkNode> m_activeChildren;

        private int m_offset;
        private int TokenIndex
        {
            get
            {
                if (m_parent == null)
                {
                    return m_offset;
                }

                return m_parent.TokenIndex + m_offset;
            }
        }

        private ForkNode(ForkNode parent)
        {
            Debug.Assert(parent != null);
            m_parent = parent;
            m_masterScanner = parent.m_masterScanner;
            m_lookAheadQueue = parent.m_lookAheadQueue;

            m_activeChildren = new List<ForkNode>();
        }

        public ForkNode CreateChildNode()
        {
            ForkNode child = new ForkNode(this);
            m_activeChildren.Add(child);

            return child;
        }

        private bool IsClosed
        {
            get
            {
                return m_masterScanner != null;
            }
        }

        private bool HasForked
        {
            get
            {
                return m_activeChildren.Count > 0;
            }
        }

        public void Close()
        {
            //step 1: remove scanner references
            m_masterScanner = null;
            m_lookAheadQueue = null;

            //step 2: check 
            
        }
    }
}
