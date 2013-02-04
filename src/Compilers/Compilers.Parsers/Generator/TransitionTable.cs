using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Generator
{


    class ActionListNode<T> : IEnumerable<T>
    {
        public T Value { get; private set; }
        private ActionListNode<T> m_nextNode;

        public ActionListNode(T value)
        {
            Value = value;
        }

        public ActionListNode<T> Append(ActionListNode<T> next)
        {
            m_nextNode = next;

            return m_nextNode;
        }

        public IEnumerator<T> GetEnumerator()
        {
            ActionListNode<T> currentNode = this;
            do
            {
                yield return currentNode.Value;

                currentNode = currentNode.m_nextNode;
            } while (currentNode != null);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static void AppendToLast(ref ActionListNode<T> list, T value)
        {
            if (list == null)
            {
                list = new ActionListNode<T>(value);
            }
            else
            {
                ActionListNode<T> head = list;

                while (head.m_nextNode != null)
                {
                    head = head.m_nextNode;
                }

                head.Append(new ActionListNode<T>(value));
            }

        }
    }

    public class TransitionTable
    {
        public int TokenCount { get; private set; }
        public int StateCount { get; private set; }
        public int ProductionCount { get; private set; }

        private ActionListNode<int>[,] m_gotoTable;
        private ActionListNode<int>[,] m_shiftTable;
        private ActionListNode<int>[,] m_reduceTable;
        private IProduction[] m_nonTerminals;

        private int m_acceptProductionIndex;

        private TransitionTable(int tokenCount, int stateCount, int productionCount)
        {
            m_gotoTable = new ActionListNode<int>[stateCount, productionCount];
            m_shiftTable = new ActionListNode<int>[stateCount, tokenCount];
            m_reduceTable = new ActionListNode<int>[stateCount, tokenCount];
        }

        public static TransitionTable Create(LR0Model model, ScannerInfo scannerInfo)
        {
            CodeContract.RequiresArgumentNotNull(model, "model");

            List<IProduction> nonterminals = new List<IProduction>();

            foreach (var production in model.ProductionInfoManager.Productions)
            {
                if (!production.IsTerminal)
                {
                    var info = model.ProductionInfoManager.GetInfo(production);

                    info.NonTerminalIndex = nonterminals.Count;
                    nonterminals.Add(production);
                }
            }

            //add one null reference to non-terminal list
            //for "accept" action in parsing
            nonterminals.Add(null);

            TransitionTable table = new TransitionTable(scannerInfo.EndOfStreamTokenIndex + 1, model.States.Count, nonterminals.Count);
            table.m_nonTerminals = nonterminals.ToArray();
            table.m_acceptProductionIndex = nonterminals.Count - 1;

            for (int i = 0; i < model.States.Count; i++)
            {
                var state = model.States[i];

                foreach (var edge in state.Edges)
                {
                    var edgeSymbol = model.ProductionInfoManager.Productions[edge.SymbolIndex];
                    var info = model.ProductionInfoManager.GetInfo(edgeSymbol);

                    if (edgeSymbol.IsTerminal)
                    {
                        //shift
                        Terminal t = edgeSymbol as Terminal;
                        int tokenIndex = t == null ? scannerInfo.EndOfStreamTokenIndex : t.Token.Index;



                        ActionListNode<int>.AppendToLast(ref table.m_shiftTable[i, tokenIndex], edge.TargetStateIndex);
                    }
                    else
                    {
                        //goto
                        ActionListNode<int>.AppendToLast(ref table.m_gotoTable[i, info.NonTerminalIndex], edge.TargetStateIndex);
                    }
                }

                //reduces
                foreach (var reduce in state.Reduces)
                {
                    Terminal t = reduce.ReduceTerminal as Terminal;
                    int tokenIndex = t == null ? scannerInfo.EndOfStreamTokenIndex : t.Token.Index;

                    var info = model.ProductionInfoManager.GetInfo(reduce.ReduceProduction);

                    ActionListNode<int>.AppendToLast(ref table.m_reduceTable[i, tokenIndex], info.NonTerminalIndex);
                }

                //accepts
                if (state.IsAcceptState)
                {
                    ActionListNode<int>.AppendToLast(ref table.m_reduceTable[i, scannerInfo.EndOfStreamTokenIndex], table.m_acceptProductionIndex);
                }

            }

            return table;
        }
    }
}
