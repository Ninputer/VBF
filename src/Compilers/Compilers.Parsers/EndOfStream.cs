using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class EndOfStream : Production<Lexeme>
    {
        public EndOfStream()
        {

        }

        public override void Accept(IProductionVisitor visitor)
        {
            visitor.VisitEndOfStream(this);
        }
    }
}
