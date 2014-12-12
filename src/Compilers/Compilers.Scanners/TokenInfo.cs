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

using System.Diagnostics;
using VBF.Compilers.Scanners.Generator;

namespace VBF.Compilers.Scanners
{
    [DebuggerDisplay("Index: {Tag.Index}  {Tag.ToString()}")]
    public class TokenInfo
    {
        internal TokenInfo(RegularExpression definition, Lexicon lexicon, Lexer state, Token tag)
        {
            Lexicon = lexicon;
            Definition = definition;
            State = state;

            Tag = tag;
        }

        public Token Tag { get; private set; }
        public Lexicon Lexicon { get; private set; }
        public Lexer State { get; private set; }
        public RegularExpression Definition { get; private set; }

        public NFAModel CreateFiniteAutomatonModel(NFAConverter converter)
        {
            NFAModel nfa = converter.Convert(Definition);

            Debug.Assert(nfa.TailState != null);

            nfa.TailState.TokenIndex = Tag.Index;

            return nfa;
        }
    }
    
}
