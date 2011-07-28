using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    [DebuggerDisplay("Index: {Tag.Index}  {Tag.ToString()}")]
    public class TokenInfo
    {
        public Token Tag { get; private set; }
        public Lexicon Lexicon { get; private set; }
        public LexerState State { get; private set; }
        public RegularExpression Definition { get; private set; }

        internal TokenInfo(RegularExpression definition, Lexicon lexicon, LexerState state, Token tag)
        {
            Lexicon = lexicon;
            Definition = definition;
            State = state;

            Tag = tag;
        }

        public NFAModel CreateFiniteAutomatonModel(NFAConverter converter)
        {
            NFAModel nfa = converter.Convert(Definition);

            Debug.Assert(nfa.TailState != null);

            nfa.TailState.TokenIndex = Tag.Index;

            return nfa;
        }
    }
    
}
