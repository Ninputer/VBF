using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    public class Token : IEquatable<Token>
    {
        public int Index { get; private set; }
        public Lexicon Lexicon { get; private set; }
        public LexerState State { get; private set; }
        public RegularExpression Definition { get; private set; }

        internal Token(RegularExpression definition, Lexicon lexicon, int index, LexerState state)
        {
            Lexicon = lexicon;
            Index = index;
            Definition = definition;
            State = state;
            //IndexInState = indexInState;
        }

        public bool Equals(Token other)
        {
            if (other == null)
            {
                return false;
            }

            return (Lexicon == other.Lexicon) && (Index == other.Index);
        }

        public override bool Equals(object obj)
        {
            Token other = obj as Token;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public NFAModel CreateFiniteAutomatonModel(NFAConverter converter)
        {
            NFAModel nfa = converter.Convert(Definition);

            Debug.Assert(nfa.TailState != null);

            nfa.TailState.TokenIndex = Index;

            return nfa;
        }
    }
}
