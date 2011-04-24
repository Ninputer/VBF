using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace VBF.Compilers.Scanners.Generator
{
    public class DFAModel
    {
        private NFAModel m_nfa;
        private List<DFAState> m_dfaStates;
        private HashSet<char> m_alphabet;
        private List<int>[] m_acceptTables;
        private Lexicon m_lexicon;
        private IComparer<char> m_charComparer;

        private DFAModel(Lexicon lexicon)
        {
            m_lexicon = lexicon;
            m_dfaStates = new List<DFAState>();
            m_alphabet = new HashSet<char>();

            //initialize accept table
            int stateCount = lexicon.LexerStateCount;
            m_acceptTables = new List<int>[stateCount];
            for (int i = 0; i < stateCount; i++)
            {
                m_acceptTables[i] = new List<int>();
            }

            //TODO
            m_charComparer = Comparer<char>.Default;
        }

        public ReadOnlyCollection<DFAState> States
        {
            get
            {
                return m_dfaStates.AsReadOnly();
            }
        }

        public ISet<char> Alphabet
        {
            get { return m_alphabet; }
        }

        public int[][] GetAcceptTables()
        {           
            return (from t in m_acceptTables select AppendEosToken(t.ToList()).ToArray()).ToArray();
        }

        private List<int> AppendEosToken(List<int> list)
        {
            //token count is the index of End OF Stream token
            //add to each accept list
            list.Add(m_lexicon.TokenCount);
            return list;
        }

        public static DFAModel Create(Lexicon lexicon)
        {
            if (lexicon == null)
            {
                return null;
            }

            DFAModel newDFA = new DFAModel(lexicon);
            newDFA.ConvertLexcionToNFA();
            newDFA.ConvertNFAToDFA();

            return newDFA;
        }

        private void ConvertLexcionToNFA()
        {
            NFAState entryState = new NFAState();
            NFAModel lexerNFA = new NFAModel();

            lexerNFA.AddState(entryState);
            foreach (var token in m_lexicon.GetTokens())
            {
                NFAModel tokenNFA = token.CreateFiniteAutomatonModel();

                entryState.AddEdge(tokenNFA.EntryEdge);
                lexerNFA.AddStates(tokenNFA.States);
            }

            lexerNFA.EntryEdge = new NFAEdge(entryState);

            m_nfa = lexerNFA;
        }

        private void SetAcceptState(int lexerStateIndex, int dfaStateIndex, int tokenIndex)
        {
            m_acceptTables[lexerStateIndex][dfaStateIndex] = tokenIndex;
        }

        private void AddDFAState(DFAState state)
        {
            m_dfaStates.Add(state);
            state.Index = m_dfaStates.Count - 1;

            for (int i = 0; i < m_acceptTables.Length; i++)
            {
                m_acceptTables[i].Add(-1);
            }

            var tokens = m_lexicon.GetTokens();
            var lexerStates = m_lexicon.GetLexerStates();
            //check accept states
            var acceptStates = (from i in state.NFAStateSet
                                let tokenIndex = m_nfa.States[i].TokenIndex
                                where tokenIndex >= 0
                                let token = tokens[tokenIndex]
                                orderby token.Index
                                group token by token.State.Index into lexerState
                                orderby lexerStates[lexerState.Key].Level
                                select lexerState).ToArray();


            if (acceptStates != null && acceptStates.Length > 0)
            {
                Queue<LexerState> stateTreeQueue = new Queue<LexerState>();

                foreach (var acceptState in acceptStates)
                {
                    int acceptTokenIndex = acceptState.First().Index;

                    //set all children lexer state's accept token to current lexer state
                    stateTreeQueue.Clear();
                    stateTreeQueue.Enqueue(lexerStates[acceptState.Key]);

                    while (stateTreeQueue.Count > 0)
                    {
                        var currentLexerState = stateTreeQueue.Dequeue();

                        foreach (var child in currentLexerState.Children)
                        {
                            stateTreeQueue.Enqueue(child);
                        }


                        SetAcceptState(currentLexerState.Index, state.Index, acceptTokenIndex);
                    }


                }
            }
        }

        private void ConvertNFAToDFA()
        {
            var nfaStates = m_nfa.States;

            //get alphabet from all edge symbols
            foreach (var s in nfaStates)
            {
                foreach (var edge in s.OutEdges)
                {
                    if (!edge.IsEmpty)
                    {
                        m_alphabet.Add(edge.Symbol.Value);
                    }
                }
            }

            //state 0 is an empty state. All invalid inputs go to state 0
            DFAState state0 = new DFAState();
            AddDFAState(state0);

            //state 1 is closure(nfaState[0])
            DFAState pre_state1 = new DFAState();
            int nfaStartIndex = m_nfa.EntryEdge.TargetState.Index;

            Debug.Assert(nfaStartIndex >= 0);

            pre_state1.NFAStateSet.Add(nfaStartIndex);

            DFAState state1 = GetClosure(pre_state1);
            AddDFAState(state1);

            //begin algorithm
            int p = 1, j = 0;
            while (j <= p)
            {
                foreach (var symbol in m_alphabet)
                {
                    DFAState e = GetDFAState(m_dfaStates[j], symbol);

                    bool isSetExist = false;
                    for (int i = 0; i <= p; i++)
                    {
                        if (e.NFAStateSet.SetEquals(m_dfaStates[i].NFAStateSet))
                        {
                            //an existing dfa state

                            DFAEdge newEdge = new DFAEdge(symbol, m_dfaStates[i]);
                            m_dfaStates[j].AddEdge(newEdge);

                            isSetExist = true;
                        }
                    }

                    if (!isSetExist)
                    {
                        //a new set of nfa states (a new dfa state)
                        p += 1;
                        AddDFAState(e);

                        DFAEdge newEdge = new DFAEdge(symbol, e);
                        m_dfaStates[j].AddEdge(newEdge);
                    }
                }

                j += 1;
            }
        }

        private DFAState GetDFAState(DFAState start, char symbol)
        {
            DFAState target = new DFAState();
            var nfaStates = m_nfa.States;

            foreach (var nfaStateIndex in start.NFAStateSet)
            {
                NFAState nfaState = nfaStates[nfaStateIndex];

                foreach (var edge in nfaState.OutEdges)
                {
                    if (!edge.IsEmpty && m_charComparer.Compare(symbol, edge.Symbol.Value) == 0)
                    {
                        int targetIndex = edge.TargetState.Index;
                        Debug.Assert(targetIndex >= 0);

                        target.NFAStateSet.Add(targetIndex);
                    }
                }
            }

            return GetClosure(target);
        }

        private DFAState GetClosure(DFAState state)
        {
            DFAState closure = new DFAState();

            var nfaStates = m_nfa.States;

            closure.NFAStateSet.UnionWith(state.NFAStateSet);
            bool changed = true;

            while (changed)
            {
                changed = false;

                List<int> lastStateSet = closure.NFAStateSet.ToList();

                foreach (var stateIndex in lastStateSet)
                {
                    NFAState nfaState = nfaStates[stateIndex];
                    foreach (var edge in nfaState.OutEdges)
                    {
                        if (edge.IsEmpty)
                        {
                            NFAState target = edge.TargetState;

                            //check whether in set
                            int targetIndex = target.Index;

                            Debug.Assert(targetIndex >= 0);

                            changed = closure.NFAStateSet.Add(targetIndex) || changed;
                        }
                    }
                }
            }

            return closure;
        }
    }
}
