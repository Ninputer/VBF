using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    public struct ForkableScanner
    {
        private int m_offset;
        private ForkableScannerCore m_core;

        internal static ForkableScanner Create(Scanner masterScanner)
        {
            return new ForkableScanner(new ForkableScannerCore(masterScanner));
        }

        private ForkableScanner(ForkableScannerCore core)
        {
            m_core = core;
            m_offset = 0;
        }

        public Lexeme Read()
        {

            Lexeme result;
            Debug.Assert(m_offset <= m_core.LookAheadQueue.Count);
            if (m_offset < m_core.LookAheadQueue.Count)
            {
                //queue is available to fetch tokens
                result = m_core.LookAheadQueue[m_offset];
            }
            else
            {
                result = m_core.MasterScanner.Read();
                m_core.LookAheadQueue.Enqueue(result);
            }

            m_offset += 1;
            return result;

        }

        public ForkableScanner Fork()
        {
            //copy instance
            return this;
        }

        public void Join(ForkableScanner scanner)
        {
            m_core = scanner.m_core;
            m_offset = scanner.m_offset;
        }

        public ScannerInfo ScannerInfo
        {
            get
            {
                if (m_core == null)
                {
                    throw new InvalidOperationException("The ForkableScanner instance is not valid. Please use ForkableScannerBuilder to create ForkableScanner.");
                }

                return m_core.MasterScanner.ScannerInfo;
            }
        }
    }
}
