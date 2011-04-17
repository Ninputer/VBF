using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners.Generator;
using System.Collections.ObjectModel;

namespace VBF.Compilers.Scanners
{
    public class Lexicon
    {
        private List<TokenIdentity> m_tokenList;
        private readonly LexerState m_defaultState;
        private List<LexerState> m_lexerStates;

        public Lexicon()
        {
            m_tokenList = new List<TokenIdentity>();
            m_lexerStates = new List<LexerState>();
            m_defaultState = new LexerState(this, 0);

            m_lexerStates.Add(m_defaultState);
        }

        internal TokenIdentity AddToken(RegularExpression definition, LexerState state, int indexInState)
        {
            int index = m_tokenList.Count;
            TokenIdentity token = new TokenIdentity(definition, this, index, state);
            m_tokenList.Add(token);

            return token;
        }

        public LexerState DefaultState
        {
            get
            {
                return m_defaultState;
            }
        }

        public ReadOnlyCollection<LexerState> GetLexerStates()
        {
            return m_lexerStates.AsReadOnly();
        }

        public ReadOnlyCollection<TokenIdentity> GetTokens()
        {
            return m_tokenList.AsReadOnly();
        }

        public int LexerStateCount
        {
            get
            {
                return m_lexerStates.Count;
            }
        }

        public int TokenCount
        {
            get
            {
                return m_tokenList.Count;
            }
        }

        public LexerState DefineLexerState(LexerState baseState)
        {
            int index = m_lexerStates.Count;
            LexerState newState = new LexerState(this, index, baseState);
            m_lexerStates.Add(newState);

            return newState;
        }

        public NFAModel CreateFiniteAutomatonModel()
        {
            NFAState entryState = new NFAState();
            NFAModel lexerNFA = new NFAModel();

            lexerNFA.AddState(entryState);
            foreach (var token in m_tokenList)
            {
                NFAModel tokenNFA = token.CreateFiniteAutomatonModel();

                entryState.AddEdge(tokenNFA.EntryEdge);
                lexerNFA.AddStates(tokenNFA.States);
            }

            lexerNFA.EntryEdge = new NFAEdge(entryState);

            return lexerNFA;
        }
    }
}
