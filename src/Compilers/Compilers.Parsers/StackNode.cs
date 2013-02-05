using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    internal class StackNode
    {
        internal StackNode PrevNode;

        internal object ReducedValue;
        internal int StateIndex;
    }
}
