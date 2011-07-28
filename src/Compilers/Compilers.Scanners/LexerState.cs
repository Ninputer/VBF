using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;

namespace VBF.Compilers.Scanners
{
    public class LexerState
    {
        private List<TokenInfo> m_tokens;
        public Lexicon Lexicon { get; private set; }
        public LexerState BaseState { get; private set; }
        public int Index { get; private set; }
        internal int Level { get; private set; }
        internal List<LexerState> Children { get; private set; }

        internal LexerState(Lexicon lexicon, int index) : this(lexicon, index, null) { }

        internal LexerState(Lexicon lexicon, int index, LexerState baseState)
        {
            Children = new List<LexerState>();
            Lexicon = lexicon;
            BaseState = baseState;
            m_tokens = new List<TokenInfo>();
            Index = index;

            if (baseState == null)
            {
                Level = 0;
            }
            else
            {
                Level = baseState.Level + 1;
                baseState.Children.Add(this);
            }
        }

        public Token DefineToken(RegularExpression regex, string description)
        {
            CodeContract.RequiresArgumentNotNull(regex, "regex");

            int indexInState = m_tokens.Count;

            TokenInfo token = Lexicon.AddToken(regex, this, indexInState, description);
            m_tokens.Add(token);

            return token.Tag;
        }

        public Token DefineToken(RegularExpression regex)
        {
            return DefineToken(regex, null);
        }

        public LexerState CreateSubState()
        {
            return Lexicon.DefineLexerState(this);
        }

    }
}
