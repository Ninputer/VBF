using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;

namespace VBF.Compilers.Scanners
{
    public class Lexer
    {
        private List<TokenInfo> m_tokens;
        public Lexicon Lexicon { get; private set; }
        public Lexer BaseLexer { get; private set; }
        public int Index { get; private set; }
        internal int Level { get; private set; }
        internal List<Lexer> Children { get; private set; }

        internal Lexer(Lexicon lexicon, int index) : this(lexicon, index, null) { }

        internal Lexer(Lexicon lexicon, int index, Lexer baseLexer)
        {
            Children = new List<Lexer>();
            Lexicon = lexicon;
            BaseLexer = baseLexer;
            m_tokens = new List<TokenInfo>();
            Index = index;

            if (baseLexer == null)
            {
                Level = 0;
            }
            else
            {
                Level = baseLexer.Level + 1;
                baseLexer.Children.Add(this);
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

        public Lexer CreateSubLexer()
        {
            return Lexicon.DefineLexer(this);
        }

    }
}
