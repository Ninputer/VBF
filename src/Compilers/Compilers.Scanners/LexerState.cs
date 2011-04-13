using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;

namespace VBF.Compilers.Scanners
{
    public class LexerState
    {
        private List<TokenIdentity> m_tokens;
        public Lexicon Lexicon { get; private set; }
        public LexerState BaseState { get; private set; }
        public int Index { get; private set; }
        internal int Level { get; private set; }

        internal LexerState(Lexicon lexicon, int index) : this(lexicon, index, null) { }

        internal LexerState(Lexicon lexicon, int index, LexerState baseState)
        {
            Lexicon = lexicon;
            BaseState = baseState;
            m_tokens = new List<TokenIdentity>();
            Index = index;

            if (baseState == null)
            {
                Level = 0;
            }
            else
            {
                Level = baseState.Level + 1;
            }
        }

        public TokenIdentity DefineToken(RegularExpression regex)
        {
            CodeContract.RequiresArgumentNotNull(regex, "regex");

            int indexInState = m_tokens.Count;

            TokenIdentity token = Lexicon.AddToken(regex, this, indexInState);
            m_tokens.Add(token);

            return token;
        }

    }
}
