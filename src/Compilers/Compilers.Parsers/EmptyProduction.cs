using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class EmptyProduction<T> : Production<T>
    {
        public T Value { get; private set; }

        public EmptyProduction(T value)
        {
            Value = value;
        }

        protected internal override void Accept<TResult>(IProductionVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
