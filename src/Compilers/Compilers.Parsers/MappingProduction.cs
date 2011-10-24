using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class MappingProduction<TSource, TReturn> : Production<TReturn>
    {
        protected internal override void Accept<TResult>(IProductionVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
