using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class ForkableScanner
    {
        private ForkNode m_node;

        internal static ForkableScanner Create(Scanner masterScanner)
        {
            ForkNode node = new ForkNode();
            node.State = new TailHeadState(node);
            node.MasterScanner = masterScanner;

            return new ForkableScanner(node);
        }

        private ForkableScanner(ForkNode node)
        {
            m_node = node;
        }

        public Lexeme Read()
        {
            return m_node.State.Read();
        }

        public ForkableScanner Fork()
        {
            if (m_node.MasterScanner == null)
            {
                throw new ObjectDisposedException(null);
            }

            var forked = m_node.State.Fork(2);

            m_node = forked[0];

            return new ForkableScanner(forked[1]);
        }

        public void Close()
        {
            if (m_node.MasterScanner == null)
            {
                throw new ObjectDisposedException(null);
            }

            m_node.Close();
        }

        public void Join(ForkableScanner child)
        {
            CodeContract.RequiresArgumentNotNull(child, "child");
            CodeContract.Requires(child.m_node.MasterScanner != null, "The scanner to join with has been closed");
            CodeContract.Requires(child.m_node.Parent == m_node.Parent, "child", "The scanner to join does not share the parent node with current scanner");

            var parent = m_node.Parent;

            //swap "this" with "child"
            var temp = m_node;
            m_node = child.m_node;
            child.m_node = temp;

            //close other children
            foreach (var otherChild in parent.Children.ToArray())
            {
                if (otherChild != m_node)
                {
                    otherChild.Close();
                }
            }
        }

        public ScannerInfo ScannerInfo
        {
            get
            {
                if (m_node.MasterScanner == null)
                {
                    throw new ObjectDisposedException(null);
                }
                return m_node.MasterScanner.ScannerInfo;
            }
        }
    }
}
