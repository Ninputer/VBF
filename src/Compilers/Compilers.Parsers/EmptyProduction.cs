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

        public override void Accept(IProductionVisitor visitor)
        {
            visitor.VisitEmpty(this);
        }
    }
}
