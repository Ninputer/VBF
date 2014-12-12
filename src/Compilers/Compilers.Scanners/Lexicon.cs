// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using VBF.Compilers.Scanners.Generator;

namespace VBF.Compilers.Scanners
{
    public class Lexicon
    {
        private readonly Lexer m_defaultState;
        private List<Lexer> m_lexerStates;
        private List<TokenInfo> m_tokenList;

        public Lexicon()
        {
            m_tokenList = new List<TokenInfo>();
            m_lexerStates = new List<Lexer>();
            m_defaultState = new Lexer(this, 0);

            m_lexerStates.Add(m_defaultState);
        }

        public Lexer Lexer
        {
            get
            {
                return m_defaultState;
            }
        }

        public int LexerCount
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

        internal TokenInfo AddToken(RegularExpression definition, Lexer state, int indexInState, string description)
        {
            int index = m_tokenList.Count;
            Token tag = new Token(index, description ?? definition.ToString(), state.Index);
            TokenInfo token = new TokenInfo(definition, this, state, tag);
            m_tokenList.Add(token);

            return token;
        }

        public ReadOnlyCollection<Lexer> GetLexers()
        {
            return m_lexerStates.AsReadOnly();
        }

        public ReadOnlyCollection<TokenInfo> GetTokens()
        {
            return m_tokenList.AsReadOnly();
        }

        internal Lexer DefineLexer(Lexer baseLexer)
        {
            int index = m_lexerStates.Count;
            Lexer newState = new Lexer(this, index, baseLexer);
            m_lexerStates.Add(newState);

            return newState;
        }

        public CompactCharSetManager CreateCompactCharSetManager()
        {
            var tokens = GetTokens();

            HashSet<char> compactableCharSet = new HashSet<char>();
            HashSet<char> uncompactableCharSet = new HashSet<char>();

            List<HashSet<char>> compactableCharSets = new List<HashSet<char>>();
            foreach (var token in tokens)
            {
                foreach (var getCharSetFunc in token.Definition.GetCompactableCharSets())
                {
                    compactableCharSets.Add(getCharSetFunc());
                }
                uncompactableCharSet.UnionWith(token.Definition.GetUncompactableCharSet());
            }

            foreach (var cset in compactableCharSets)
            {
                compactableCharSet.UnionWith(cset);
            }

            compactableCharSet.ExceptWith(uncompactableCharSet);

            Dictionary<HashSet<int>, ushort> compactClassDict = new Dictionary<HashSet<int>, ushort>(HashSet<int>.CreateSetComparer());
            ushort compactCharIndex = 1;
            ushort[] compactClassTable = new ushort[65536];

            foreach (var c in uncompactableCharSet)
            {
                ushort index = compactCharIndex++;
                compactClassTable[(int)c] = index;
            }

            foreach (var c in compactableCharSet)
            {
                HashSet<int> setofcharset = new HashSet<int>();
                for (int i = 0; i < compactableCharSets.Count; i++)
                {
                    var set = compactableCharSets[i];
                    if (set.Contains(c)) setofcharset.Add(i);
                }

                if (compactClassDict.ContainsKey(setofcharset))
                {
                    //already exist
                    ushort index = compactClassDict[setofcharset];

                    compactClassTable[(int)c] = index;
                }
                else
                {
                    ushort index = compactCharIndex++;
                    compactClassDict.Add(setofcharset, index);

                    compactClassTable[(int)c] = index;
                }
            }
            return new CompactCharSetManager(compactClassTable, compactCharIndex);
        }

        public ScannerInfo CreateScannerInfo()
        {
            DFAModel dfa = DFAModel.Create(this);
            CompressedTransitionTable ctt = CompressedTransitionTable.Compress(dfa);

            return new ScannerInfo(ctt.TransitionTable, ctt.CharClassTable, dfa.GetAcceptTables(), m_tokenList.Count);
        }
    }
}
