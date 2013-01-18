using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers.ShiftReduce.Generator
{
    class LRStateCore
    {
        public IProduction Production { get; private set; }
        public int Location { get; private set; }


    }
}
