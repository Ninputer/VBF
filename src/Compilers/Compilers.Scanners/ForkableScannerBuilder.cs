using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class ForkableScannerBuilder
    {

        public CompilationErrorManager ErrorManager { get; set; }

        public bool RecoverErrors { get; set; }

        public int LexicalErrorId { get; set; }

        private ScannerInfo m_info;

        private int[] m_skipTokens;

        public ForkableScannerBuilder(ScannerInfo info)
        {
            CodeContract.RequiresArgumentNotNull(info, "info");

            m_info = info;
            m_skipTokens = new int[0];
        }

        public ScannerInfo ScannerInfo
        {
            get
            {
                return m_info;
            }
            set
            {
                CodeContract.RequiresArgumentNotNull(value, "value");
                m_info = value;
            }
        }

        public void SetSkipTokens(params int[] skipTokenIndices)
        {
            m_skipTokens = skipTokenIndices;
        }

        public ForkableScanner Create(SourceReader source)
        {
            CodeContract.RequiresArgumentNotNull(source, "source");

            Scanner masterScanner = new Scanner(ScannerInfo);
            masterScanner.SetSource(source);
            masterScanner.SetSkipTokens(m_skipTokens);
            masterScanner.ErrorManager = ErrorManager;
            masterScanner.RecoverErrors = RecoverErrors;
            masterScanner.LexicalErrorId = LexicalErrorId;

            return ForkableScanner.Create(masterScanner);
        }
    }
}
