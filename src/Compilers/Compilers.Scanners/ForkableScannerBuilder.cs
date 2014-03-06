using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class ForkableScannerBuilder
    {

        public CompilationErrorList ErrorList { get; set; }

        public bool RecoverErrors { get; set; }

        public int LexicalErrorId { get; set; }

        private ScannerInfo m_info;

        private int[] m_triviaTokens;

        public ForkableScannerBuilder(ScannerInfo info)
        {
            CodeContract.RequiresArgumentNotNull(info, "info");

            m_info = info;
            m_triviaTokens = new int[0];
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

        public void SetTriviaTokens(params int[] triviaTokenIndices)
        {
            m_triviaTokens = triviaTokenIndices;
        }

        public ForkableScanner Create(SourceReader source)
        {
            CodeContract.RequiresArgumentNotNull(source, "source");

            Scanner masterScanner = new Scanner(ScannerInfo);
            masterScanner.SetSource(source);
            masterScanner.SetTriviaTokens(m_triviaTokens);
            masterScanner.ErrorList = ErrorList;
            masterScanner.RecoverErrors = RecoverErrors;
            masterScanner.LexicalErrorId = LexicalErrorId;

            return ForkableScanner.Create(masterScanner);
        }
    }
}
