using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public abstract class Production<T>
    {
        protected internal abstract void Accept<TResult>(IProductionVisitor<TResult> visitor);
    }
}
