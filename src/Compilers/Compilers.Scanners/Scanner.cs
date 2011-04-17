using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class Scanner
    {
        //consts
        public const int InvalidTokenIndex = -1;

        private FiniteAutomationEngine m_engine;
        private SourceCode m_source;

        private int m_lastAcceptedTokenIndex;
        private SourceLocation m_lastTokenStart;

        public Scanner(FiniteAutomationEngine engine)
        {
            CodeContract.RequiresArgumentNotNull(engine, "engine");

            m_engine = engine;

            Initialize();
        }

        private void Initialize()
        {
            m_engine.Reset();
            m_lastAcceptedTokenIndex = InvalidTokenIndex;
        }

        public void SetSource(SourceCode source)
        {
            CodeContract.RequiresArgumentNotNull(source, "source");
            m_source = source;

            Initialize();
        }

        public int Peek()
        {
            throw new NotImplementedException();
        }

        public int Peek2()
        {
            throw new NotImplementedException();
        }

        public int PeekInState(int lexerStateIndex)
        {
            throw new NotImplementedException();
        }

        public int Peek2InState(int lexerStateIndex)
        {
            throw new NotImplementedException();
        }

        public Lexeme Read()
        {
            throw new NotImplementedException();
        }
    }
}
