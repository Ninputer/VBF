using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    class ForkNode1
    {
        private Scanner m_masterScanner;
        private CacheQueue<Lexeme> m_lookAheadQueue;

        private ForkNode1 m_parent;
        private List<ForkNode1> m_activeChildren;

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

        private ForkNode1(ForkNode1 parent)
        {
            Debug.Assert(parent != null);
            m_parent = parent;
            m_masterScanner = parent.m_masterScanner;
            m_lookAheadQueue = parent.m_lookAheadQueue;

            m_activeChildren = new List<ForkNode1>();
        }

        public ForkNode1 CreateChildNode()
        {
            ForkNode1 child = new ForkNode1(this);
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

    class ForkNode
    {
        public Scanner MasterScanner { get; set; }
        public CacheQueue<Lexeme> LookAheadQueue { get; private set; }
        public List<ForkNode> Children { get; private set; }
        public ForkNode Parent { get; private set; }
        public int Offset { get; set; }

        private NodeState m_state;

        public ForkNode(ForkNode parent)
        {
            if (parent == null)
            {
                LookAheadQueue = new CacheQueue<Lexeme>();
            }
            else
            {
                LookAheadQueue = parent.LookAheadQueue;
                MasterScanner = parent.MasterScanner;
            }

            Children = new List<ForkNode>();
            Parent = parent;
        }

        public int Position
        {
            get
            {
                if (Parent == null)
                {
                    return Offset;
                }

                return Parent.Position + Offset;
            }
        }


    }

    abstract class NodeState
    {
        protected ForkNode Node { get; private set; }
        protected NodeState(ForkNode node)
        {
            Node = node;
        }

        public abstract ForkNode[] Fork(int count);
        public abstract void Close();
        public abstract Lexeme Read();
    }

    class TailState
    {
        void Forward(ForkNode child) { }
    }

    class HeadState : NodeState
    {
        public HeadState(ForkNode node) : base(node) { }


        public override ForkNode[] Fork(int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override Lexeme Read()
        {
            throw new NotImplementedException();
        }
    }

    class BranchState
    {

    }

    class TailHeadState
    {

    }
}
