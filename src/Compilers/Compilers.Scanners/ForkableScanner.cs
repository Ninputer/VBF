using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class ForkableScanner
    {
        private ForkNode m_node;

        public static ForkableScanner Create(ScannerInfo scannerInfo, SourceReader source, params int[] skipTokens)
        {
            ForkNode node = new ForkNode();
            node.State = new TailHeadState(node);
            node.MasterScanner = new Scanner(scannerInfo);
            node.MasterScanner.SetSource(source);
            node.MasterScanner.SetSkipTokens(skipTokens);

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
