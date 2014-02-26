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
        private Scanner m_masterScanner;

        internal static ForkableScanner Create(Scanner masterScanner)
        {
            return new ForkableScanner(masterScanner);
        }

        private ForkableScanner(Scanner masterScanner)
        {
            m_masterScanner = masterScanner;
            m_offset = 0;
        }

        public Lexeme Read()
        {

            Lexeme result;
            Debug.Assert(m_offset <= m_masterScanner.History.Count);
            if (m_offset < m_masterScanner.History.Count)
            {
                //queue is available to fetch tokens
                result = m_masterScanner.History[m_offset];
            }
            else
            {
                result = m_masterScanner.Read();
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
            m_masterScanner = scanner.m_masterScanner;
            m_offset = scanner.m_offset;
        }

        public ScannerInfo ScannerInfo
        {
            get
            {
                if (m_masterScanner == null)
                {
                    throw new InvalidOperationException("The ForkableScanner instance is not valid. Please use ForkableScannerBuilder to create ForkableScanner.");
                }

                return m_masterScanner.ScannerInfo;
            }
        }
    }
}
