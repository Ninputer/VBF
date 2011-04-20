using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public struct ScannerInfo
    {
        internal int[][] TransitionTable;
        internal ushort[] CharClassTable;
        internal int[][] AcceptTables;
        internal int TokenCount;

        public ScannerInfo(int[][] transitionTable, ushort[] charClassTable, int[][] acceptTables, int tokenCount)
            : this()
        {
            TransitionTable = transitionTable;
            CharClassTable = charClassTable;
            AcceptTables = acceptTables;
            TokenCount = tokenCount;
        }
    }
}
