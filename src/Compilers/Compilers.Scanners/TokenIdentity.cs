using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    public class TokenIdentity : IEquatable<TokenIdentity>
    {
        public int Index { get; private set; }
        internal int IndexInState { get; private set; }
        public Lexicon Lexicon { get; private set; }
        public LexerState State { get; private set; }
        public RegularExpression Definition { get; private set; }

        internal TokenIdentity(RegularExpression definition, Lexicon lexicon, int index, LexerState state, int indexInState)
        {
            Lexicon = lexicon;
            Index = index;
            Definition = definition;
            State = state;
            IndexInState = indexInState;
        }

        public bool Equals(TokenIdentity other)
        {
            if (other == null)
            {
                return false;
            }

            return (Lexicon == other.Lexicon) && (Index == other.Index);
        }

        public override bool Equals(object obj)
        {
            TokenIdentity other = obj as TokenIdentity;

            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public NFAModel CreateFiniteAutomatonModel()
        {
            NFAModel nfa = NFAConverter.Default.Convert(Definition);

            Debug.Assert(nfa.TailState != null);

            nfa.TailState.TokenIdentityIndex = Index;

            return nfa;
        }
    }
}
