using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class EmptyProduction<T> : ProductionBase<T>
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

        public override string ToString()
        {
            return DebugName + " ::= ε";
        }

        public override string DebugName
        {
            get
            {
                return "E" + DebugNameSuffix;
            }
        }
    }
}
