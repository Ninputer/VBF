using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners.Generator
{
    public struct NFAEdge
    {
        public char? Symbol { get; private set; }
        public NFAState TargetState { get; private set; }

        public NFAEdge(char symbol, NFAState targetState)
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
                return Symbol.HasValue;
            }
        }

    }
}
