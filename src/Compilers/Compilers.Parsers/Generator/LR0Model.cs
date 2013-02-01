using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.Generator
{
    public class LR0Model
    {
        private ProductionInfoManager m_infoManager;
        private List<LR0State> m_states;
        private ClosureVisitor m_closureVisitor = new ClosureVisitor();

        public LR0Model(ProductionInfoManager infoManager)
        {
            CodeContract.RequiresArgumentNotNull(infoManager, "infoManager");

            m_infoManager = infoManager;
            m_states = new List<LR0State>();
        }

        public void BuildModel()
        {
            ISet<LR0Item> initStateSet = new HashSet<LR0Item>();
            initStateSet.Add(new LR0Item(m_infoManager.GetInfo(m_infoManager.RootProduction).Index, 0));

            initStateSet = GetClosure(initStateSet);

            LR0State initState = new LR0State(initStateSet);
            initState.Index = 0;

            m_states.Add(initState);

            DotSymbolVisitor dotSymbol = new DotSymbolVisitor();
            List<LR0State> statesToAdd = new List<LR0State>();
            bool isChanged = false;

            //build shifts and gotos
            do
            {
                isChanged = false;
                statesToAdd.Clear();

                foreach (var state in m_states)
                {
                    foreach (var item in state.ItemSet)
                    {
                        var production = m_infoManager.Productions[item.ProductionIndex];
                        var info = m_infoManager.GetInfo(production);

                        dotSymbol.DotLocation = item.DotLocation;
                        production.Accept(dotSymbol);
                        foreach (var symbol in dotSymbol.Symbols)
                        {
                            var targetStateSet = GetGoto(state.ItemSet, symbol);
                            LR0State targetState;
                            //check if the target state is exist
                            bool exist = CheckExist(statesToAdd, targetStateSet, out targetState);

                            if (exist)
                            {
                                //check edges
                                isChanged = state.AddEdge(symbol, targetState) || isChanged;
                            }
                            else
                            {
                                isChanged = true;

                                //create new state
                                targetState = new LR0State(targetStateSet);

                                statesToAdd.Add(targetState);

                                //create edge for it
                                state.AddEdge(symbol, targetState);
                            }
                        }
                    }
                }

                //add new states
                for (int i = 0; i < statesToAdd.Count; i++)
                {
                    statesToAdd[i].Index = m_states.Count;
                    m_states.Add(statesToAdd[i]);
                }

            } while (isChanged);


            //build reduces
            foreach (var state in m_states)
            {
                foreach (var item in state.ItemSet)
                {
                    var production = m_infoManager.Productions[item.ProductionIndex];
                    var info = m_infoManager.GetInfo(production);

                    if (item.DotLocation == info.SymbolCount)
                    {
                        foreach (var followSymbol in info.Follow)
                        {
                            //reduce
                            state.AddReduce(followSymbol, production);
                        }

                    }
                }
            }
        }

        private bool CheckExist(IList<LR0State> addingStates, ISet<LR0Item> set, out LR0State targetState)
        {
            bool exist = false;
            targetState = null;

            foreach (var state in m_states)
            {
                if (set.SetEquals(state.ItemSet))
                {
                    exist = true;
                    targetState = state;
                    break;
                }
            }

            foreach (var state in addingStates)
            {
                if (set.SetEquals(state.ItemSet))
                {
                    exist = true;
                    targetState = state;
                    break;
                }
            }

            return exist;
        }

        private ISet<LR0Item> GetClosure(ISet<LR0Item> initSet)
        {
            m_closureVisitor.LR0ItemSet = initSet;

            do
            {
                m_closureVisitor.IsChanged = false;
                foreach (var item in initSet.ToArray())
                {
                    m_closureVisitor.DotLocation = item.DotLocation;
                    m_infoManager.Productions[item.ProductionIndex].Accept(m_closureVisitor);
                }
            } while (m_closureVisitor.IsChanged);

            return m_closureVisitor.LR0ItemSet;
        }

        private ISet<LR0Item> GetGoto(IEnumerable<LR0Item> state, IProduction symbol)
        {
            ISet<LR0Item> resultSet = new HashSet<LR0Item>();

            foreach (var item in state)
            {
                var production = m_infoManager.Productions[item.ProductionIndex];
                var info = m_infoManager.GetInfo(production);

                if (item.DotLocation < info.SymbolCount)
                {
                    resultSet.Add(new LR0Item(info.Index, item.DotLocation + 1));
                }
            }

            return GetClosure(resultSet);
        }
    }
}
