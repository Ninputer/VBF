using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    [DebuggerDisplay("Index: {Index}  {ToString()}")]
    public class Token : IEquatable<Token>
    {
        public int Index { get; private set; }
        public Lexicon Lexicon { get; private set; }
        public LexerState State { get; private set; }
        public RegularExpression Definition { get; private set; }
        public string Description { get; private set; }

        internal Token(RegularExpression definition, Lexicon lexicon, int index, LexerState state, string description)
        {
            Lexicon = lexicon;
            Index = index;
            Definition = definition;
            State = state;
            Description = description;
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

        public override string ToString()
        {
            if (!String.IsNullOrEmpty(Description))
            {
                return Description;
            }
            else
            {
                return Definition.ToString();
            }
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
