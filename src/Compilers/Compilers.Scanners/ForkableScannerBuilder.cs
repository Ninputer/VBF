// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace VBF.Compilers.Scanners
{
    public class ForkableScannerBuilder
    {
        private ScannerInfo m_info;

        private int[] m_triviaTokens;

        public ForkableScannerBuilder(ScannerInfo info)
        {
            CodeContract.RequiresArgumentNotNull(info, "info");

            m_info = info;
            m_triviaTokens = new int[0];
        }

        public CompilationErrorList ErrorList { get; set; }

        public bool RecoverErrors { get; set; }

        public int LexicalErrorId { get; set; }

        public bool ThrowAtReadingAfterEndOfStream { get; set; }

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
            masterScanner.ThrowAtReadingAfterEndOfStream = ThrowAtReadingAfterEndOfStream;

            return ForkableScanner.Create(masterScanner);
        }
    }
}
