using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners.Generator
{
    public struct DFAEdge
    {
        public int Symbol { get; private set; }
        public DFAState TargetState { get; private set; }

        public DFAEdge(int symbol, DFAState targetState)
            : this()
        {
            Symbol = symbol;
            TargetState = targetState;
        }

    }
}
