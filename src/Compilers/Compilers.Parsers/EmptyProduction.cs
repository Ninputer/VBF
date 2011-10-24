using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class EmptyProduction<T> : Production<T>
    {
        protected internal override void Accept<TResult>(IProductionVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
