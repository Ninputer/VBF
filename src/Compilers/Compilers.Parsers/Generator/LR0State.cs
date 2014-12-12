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

namespace VBF.Compilers.Parsers.Generator
{
    public class LR0State
    {
        private ISet<LR0Edge> m_edges;
        private ISet<LR0Item> m_itemSet;
        private int? m_maxReducingLexer;
        private int? m_maxShiftingLexer;
        private List<ReduceAction> m_reduces;

        internal LR0State(ISet<LR0Item> itemSet)
        {
            m_itemSet = itemSet;
            m_reduces = new List<ReduceAction>();
            m_edges = new HashSet<LR0Edge>();
            m_maxShiftingLexer = null;
            m_maxReducingLexer = null;

            //for generator only
            PossibleEdges = new List<KeyValuePair<IProduction, ISet<LR0Item>>>();
        }

        //top stack symbol conversion rules
        //executes at goto action
        //private List<SymbolConversion> m_conversions;

        //index in transition table (row number)
        public int Index { get; internal set; }

        public bool IsAcceptState { get; internal set; }

        internal List<KeyValuePair<IProduction, ISet<LR0Item>>> PossibleEdges { get; private set; }

        public IEnumerable<LR0Item> ItemSet
        {
            get
            {
                return m_itemSet;
            }
        }

        public IEnumerable<LR0Edge> Edges
        {
            get
            {
                return m_edges;
            }
        }

        public IReadOnlyList<ReduceAction> Reduces
        {
            get
            {
                return m_reduces;
            }
        }

        public int? MaxShiftingLexer
        {
            get
            {
                return m_maxShiftingLexer;
            }
        }

        public int? MaxReducingLexer
        {
            get
            {
                return m_maxReducingLexer;
            }
        }

        internal bool AddEdge(IProduction symbol, LR0State targetState)
        {
            ProductionBase production = symbol as ProductionBase;

            return m_edges.Add(new LR0Edge(Index, production.Info.Index, targetState.Index));
        }

        internal void AddReduce(IProduction reduceSymbol, IProduction reduceProduction)
        {
            m_reduces.Add(new ReduceAction(reduceSymbol, reduceProduction));
        }

        internal void AddShiftingLexer(int lexerIndex)
        {
            if (m_maxShiftingLexer == null)
            {
                m_maxShiftingLexer = lexerIndex;
            }
            else
            {
                if (m_maxShiftingLexer.Value < lexerIndex)
                {
                    m_maxShiftingLexer = lexerIndex;
                }
            }
        }

        internal void AddReducingLexer(int lexerIndex)
        {
            if (m_maxReducingLexer == null)
            {
                m_maxReducingLexer = lexerIndex;
            }
            else
            {
                if (m_maxReducingLexer.Value < lexerIndex)
                {
                    m_maxReducingLexer = lexerIndex;
                }
            }
        }

        public override string ToString()
        {
            return "state" + Index;
        }
    }
}
