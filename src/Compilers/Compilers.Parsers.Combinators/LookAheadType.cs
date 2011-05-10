using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers.Combinators
{
    public enum LookAheadType
    {
        Shift,
        Split,
        Reduce,
        Found
    }
}
