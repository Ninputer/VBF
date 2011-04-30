using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners.Generator
{
    public struct NFAEdge
    {
        public int? Symbol { get; private set; }
        public NFAState TargetState { get; private set; }

        public NFAEdge(int symbol, NFAState targetState)
            : this()
        {
            Symbol = symbol;
            TargetState = targetState;
        }

        public NFAEdge(NFAState targetState)
            : this()
        {
            TargetState = targetState;
        }

        public bool IsEmpty
        {
            get
            {
                return !Symbol.HasValue;
            }
        }

    }
}
