using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ParserContext
    {
        public ForkableScanner Scanner { get; private set; }
    }
}
