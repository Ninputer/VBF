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

using System;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    public struct ForkableScanner
    {
        private Scanner m_masterScanner;
        private int m_offset;

        private ForkableScanner(Scanner masterScanner)
        {
            m_masterScanner = masterScanner;
            m_offset = 0;
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

        internal static ForkableScanner Create(Scanner masterScanner)
        {
            return new ForkableScanner(masterScanner);
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
    }
}
