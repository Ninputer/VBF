using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners.Generator
{
    class CompressedTransitionTable
    {
        IList<DFAState> m_dfaStates;
        CompactCharSetManager m_compactCharSetManager;

        //ISet<char> m_alphabet;

        ushort[] m_charClassTable;
        Dictionary<string, ushort> m_stateSetDict;

        int[][] m_compressedTransitionTable;

        public int[][] TransitionTable
        {
            get { return m_compressedTransitionTable; }
        }

        public ushort[] CharClassTable
        {
            get { return m_charClassTable; }
        }

        public static CompressedTransitionTable Compress(DFAModel dfa)
        {
            if (dfa == null)
            {
                return null;
            }

            CompressedTransitionTable compressor = new CompressedTransitionTable(dfa);
            compressor.Compress();

            return compressor;
        }

        private CompressedTransitionTable(DFAModel dfa)
        {
            m_charClassTable = new ushort[65536];

            m_stateSetDict = new Dictionary<string, ushort>();

            m_dfaStates = dfa.States;

            m_compactCharSetManager = dfa.CompactCharSetManager;

            m_compressedTransitionTable = new int[m_dfaStates.Count][];
        }

        private void Compress()
        {
            Dictionary<int, int>[] transitionTable = new Dictionary<int, int>[m_dfaStates.Count];



            for (int i = 0; i < m_dfaStates.Count; i++)
            {
                transitionTable[i] = new Dictionary<int, int>();

                foreach (var edge in m_dfaStates[i].OutEdges)
                {
                    transitionTable[i].Add(edge.Symbol, edge.TargetState.Index);
                }
            }

            List<int[]> transitionColumnTable = new List<int[]>();
            var compactCharMapTable = m_compactCharSetManager.CreateCompactCharMapTable();

            //valid chars
            for (int i = m_compactCharSetManager.MinClassIndex; i <= m_compactCharSetManager.MaxClassIndex; i++)
            {
                int[] columnSequence = (from row in transitionTable select row[i]).ToArray();
                StringBuilder signatureBuilder = new StringBuilder();

                foreach (var item in columnSequence)
                {
                    signatureBuilder.Append(item);
                    signatureBuilder.Append(',');
                }

                string columnSignature = signatureBuilder.ToString();

                if (m_stateSetDict.ContainsKey(columnSignature))
                {
                    //already exist
                    foreach (var c in compactCharMapTable[i])
                    {
                        m_charClassTable[c] = m_stateSetDict[columnSignature];
                    }
                }
                else
                {
                    //there is at most 65536 char classes
                    ushort nextIndex = (ushort)transitionColumnTable.Count;

                    //a new char set

                    transitionColumnTable.Add(columnSequence);
                    m_stateSetDict[columnSignature] = nextIndex;

                    foreach (var c in compactCharMapTable[i])
                    {
                        m_charClassTable[c] = nextIndex;
                    }
                }
            }

            //create a char set for all invalid chars
            //navigate them all to dfa state #0 (the invalid state)
            int[] invalidColumn = new int[m_dfaStates.Count];
            ushort invalidIndex = (ushort)transitionColumnTable.Count;

            transitionColumnTable.Add(invalidColumn);

            foreach (var c in compactCharMapTable[0])
            {
                m_charClassTable[c] = invalidIndex;
            }

            //generate real compressed transition table

            for (int i = 0; i < m_dfaStates.Count; i++)
            {
                m_compressedTransitionTable[i] = (from column in transitionColumnTable select column[i]).ToArray();
            }
        }
    }
}
