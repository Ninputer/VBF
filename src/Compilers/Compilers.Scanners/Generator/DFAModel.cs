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
        private NFAModel m_baseNfa;
        private List<DFAState> m_states;
        private HashSet<char> m_alphabet;
        private IComparer<char> m_charComparer;

        private DFAModel(NFAModel baseNfa)
        {
            m_states = new List<DFAState>();
            m_alphabet = new HashSet<char>();
            m_baseNfa = baseNfa;

            //TODO:
            m_charComparer = Comparer<char>.Default;
        }

        public ReadOnlyCollection<DFAState> States
        {
            get
            {
                return m_states.AsReadOnly();
            }
        }

        private void AddState(DFAState state)
        {
            m_states.Add(state);
            state.Index = m_states.Count - 1;
        }

        public static DFAModel FromNFA(NFAModel nfa)
        {
            if (nfa == null) return null;

            DFAModel dfa = new DFAModel(nfa);

            var nfaStates = nfa.States;

            //get alphabet from all edge symbols
            foreach (var s in nfaStates)
            {
                foreach (var edge in s.OutEdges)
                {
                    if (!edge.IsEmpty)
                    {
                        dfa.m_alphabet.Add(edge.Symbol.Value);
                    }
                }
            }

            //state 0 is an empty state. All invalid inputs go to state 0
            DFAState state0 = new DFAState();
            dfa.AddState(state0);

            //state 1 is closure(nfaState[0])
            DFAState pre_state1 = new DFAState();
            int nfaStartIndex = nfa.EntryEdge.TargetState.Index;

            Debug.Assert(nfaStartIndex >= 0);

            pre_state1.NFAStateSet.Add(nfaStartIndex);

            DFAState state1 = dfa.GetClosure(pre_state1);
            dfa.AddState(state1);

            //begin algorithm
            int p = 1, j = 0;
            while (j <= p)
            {
                foreach (var symbol in dfa.m_alphabet)
                {
                    DFAState e = dfa.GetDFAState(dfa.m_states[j], symbol);

                    bool isSetExist = false;
                    for (int i = 0; i <= p; i++)
                    {
                        if (e.NFAStateSet.SetEquals(dfa.m_states[i].NFAStateSet))
                        {
                            //an existing dfa state

                            DFAEdge newEdge = new DFAEdge(symbol, dfa.m_states[i]);
                            dfa.m_states[j].AddEdge(newEdge);

                            isSetExist = true;
                        }
                    }

                    if (!isSetExist)
                    {
                        //a new set of nfa states (a new dfa state)
                        p += 1;
                        dfa.AddState(e);

                        DFAEdge newEdge = new DFAEdge(symbol, e);
                        dfa.m_states[j].AddEdge(newEdge);
                    }
                }

                j += 1;
            }

            return dfa;
        }

        private DFAState GetDFAState(DFAState start, char symbol)
        {
            DFAState target = new DFAState();
            var nfaStates = m_baseNfa.States;

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

            var nfaStates = m_baseNfa.States;

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
