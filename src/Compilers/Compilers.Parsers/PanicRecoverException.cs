using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    class PanicRecoverException : Exception
    {
        public ISet<IProduction> PossibleFollow { get; private set; }
        public PanicRecoverException(ISet<IProduction> follow)
        {
            PossibleFollow = follow;
        }
    }
}
