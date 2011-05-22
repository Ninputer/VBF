using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Parsers.Combinators;
using VBF.Compilers.Scanners;
using VBF.Compilers;
using System.IO;

namespace VBF.MiniSharp
{
    public abstract class ParserFrame<T>
    {
        private ParserContext m_context;
        private readonly CompilationErrorManager m_errorManager;
        private Lexicon m_lexicon;
        private ScannerInfo m_scannerInfo;
        private ForkableScannerBuilder m_scannerBuilder;
        private Parser<T> m_parser;
        private ParserRunner<T> m_parserRunner;
        private readonly int m_missingTokenErrorId;
        private readonly int m_unexpectedTokenErrorId;
        private readonly int m_lexicalErrorId;

        private bool m_isInitialized = false;
        private List<int> m_skippedTokens;

        protected ParserFrame(CompilationErrorManager errorManager, int lexicalErrorId, int missingTokenErrorId, int unexpectedTokenErrorId)
        {
            m_errorManager = errorManager;

            m_missingTokenErrorId = missingTokenErrorId;
            m_unexpectedTokenErrorId = unexpectedTokenErrorId;
            m_lexicalErrorId = lexicalErrorId;

            m_skippedTokens = new List<int>();
        }

        private void Initialize()
        {
            m_lexicon = new Lexicon();

            OnDefineLexer(m_lexicon, m_skippedTokens);

            m_scannerInfo = m_lexicon.CreateScannerInfo();

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
            m_scannerBuilder.SetSkipTokens(m_skippedTokens.ToArray());
            m_scannerBuilder.ErrorManager = m_errorManager;
            m_scannerBuilder.RecoverErrors = true;
            m_scannerBuilder.LexicalErrorId = m_lexicalErrorId;

            m_isInitialized = true;
        }

        protected abstract void OnDefineLexer(Lexicon lexicon, ICollection<int> skippedTokens);

        protected virtual void OnDefineParserErrors(CompilationErrorManager errorManager)
        {
            errorManager.DefineError(m_lexicalErrorId, 0, CompilationStage.Scanning, "Invalid token: {0}");
            m_context.DefineDefaultCompilationErrorInfo(0);
        }

        protected abstract Parser<T> OnDefineParser();

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

        public T Parse(SourceReader source)
        {
            if (!m_isInitialized)
            {
                Initialize();
            }

            ForkableScanner scanner = m_scannerBuilder.Create(source);

            return m_parserRunner.Run(scanner);
        }

        public T Parse(TextReader source)
        {
            return Parse(new SourceReader(source));
        }

        public T Parse(string source)
        {
            return Parse(new StringReader(source));
        }
    }
}
