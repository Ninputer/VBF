using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    internal class StackNode
    {
        internal readonly StackNode PrevNode;
        internal object ReducedValue;
        internal readonly int StateIndex;

        public StackNode(int stateIndex, StackNode prev, object value)
        {
            StateIndex = stateIndex;
            PrevNode = prev;
            ReducedValue = value;
        }
    }
}
