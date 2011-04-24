using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public class ScannerInfo
    {
        internal int[][] TransitionTable;
        internal ushort[] CharClassTable;
        internal int[][] AcceptTables;
        internal int TokenCount;

        public int EndOfStreamTokenIndex { get; private set; }

        internal ScannerInfo(int[][] transitionTable, ushort[] charClassTable, int[][] acceptTables, int tokenCount)
        {
            TransitionTable = transitionTable;
            CharClassTable = charClassTable;
            AcceptTables = acceptTables;
            TokenCount = tokenCount;
        }
    }
}
