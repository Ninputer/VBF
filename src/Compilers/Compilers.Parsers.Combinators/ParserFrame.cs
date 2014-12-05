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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public abstract class ParserFrame<T>
    {
        private readonly CompilationErrorManager m_errorManager;
        private readonly int m_lexicalErrorId;
        private readonly int m_missingTokenErrorId;
        private readonly int m_unexpectedTokenErrorId;
        private ParserContext m_context;

        private bool m_isInitialized;
        private Lexicon m_lexicon;
        private Parser<T> m_parser;
        private ParserRunner<T> m_parserRunner;
        private ForkableScannerBuilder m_scannerBuilder;
        private ScannerInfo m_scannerInfo;
        private List<Token> m_triviaTokens;

        protected ParserFrame(CompilationErrorManager errorManager, int lexicalErrorId, int missingTokenErrorId, int unexpectedTokenErrorId)
        {
            CodeContract.RequiresArgumentNotNull(errorManager, "errorManager");

            m_errorManager = errorManager;

            m_missingTokenErrorId = missingTokenErrorId;
            m_unexpectedTokenErrorId = unexpectedTokenErrorId;
            m_lexicalErrorId = lexicalErrorId;

            m_triviaTokens = new List<Token>();
        }

        protected ScannerInfo ScannerInfo
        {
            get
            {
                return m_scannerInfo;
            }
        }

        protected ParserContext Context
        {
            get
            {
                return m_context;
            }
        }

        public void Initialize()
        {
            if (!m_isInitialized)
            {
                OnInitialize();
            }
        }

        private void OnInitialize()
        {
            m_lexicon = new Lexicon();

            OnDefineLexer(m_lexicon, m_triviaTokens);

            m_scannerInfo = OnCreateScannerInfo();

            var parser = OnDefineParser();

            if (parser == null)
            {
                throw new InvalidOperationException("Parser not defined");
            }

            m_parser = parser.SuffixedBy(Parsers.Eos());

            m_context = new ParserContext(m_errorManager, m_unexpectedTokenErrorId, m_missingTokenErrorId);

            OnDefineParserErrors(m_errorManager);

            m_parserRunner = new ParserRunner<T>(m_parser, m_context);

            m_scannerBuilder = new ForkableScannerBuilder(m_scannerInfo);
            m_scannerBuilder.SetTriviaTokens(m_triviaTokens.Select(t => t.Index).ToArray());           
            m_scannerBuilder.RecoverErrors = true;
            m_scannerBuilder.LexicalErrorId = m_lexicalErrorId;

            m_isInitialized = true;
        }

        protected virtual ScannerInfo OnCreateScannerInfo()
        {
            return m_lexicon.CreateScannerInfo();
        }

        protected abstract void OnDefineLexer(Lexicon lexicon, ICollection<Token> triviaTokens);

        protected virtual void OnDefineParserErrors(CompilationErrorManager errorManager)
        {
            errorManager.DefineError(m_lexicalErrorId, 0, CompilationStage.Scanning, "Invalid token: {0}");
            m_context.DefineDefaultCompilationErrorInfo(0);
        }

        protected abstract Parser<T> OnDefineParser();

        public T Parse(SourceReader source, CompilationErrorList errorList)
        {
            CodeContract.RequiresArgumentNotNull(source, "source");

            if (!m_isInitialized)
            {
                OnInitialize();
            }

            m_scannerBuilder.ErrorList = errorList;
            m_context.ErrorList = errorList;
            ForkableScanner scanner = m_scannerBuilder.Create(source);

            return m_parserRunner.Run(scanner);
        }

        public T Parse(TextReader source, CompilationErrorList errorList)
        {
            CodeContract.RequiresArgumentNotNull(source, "source");
            return Parse(new SourceReader(source), errorList);
        }

        public T Parse(string source, CompilationErrorList errorList)
        {
            CodeContract.RequiresArgumentNotNull(source, "source");
            return Parse(new StringReader(source), errorList);
        }
    }
}
