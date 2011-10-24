using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class ConcatenationProduction<T1, T2, TR> : Production<TR>
    {
        protected internal override void Accept<TResult>(IProductionVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
