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

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument)
        {
            return visitor.VisitEmpty(this, argument);
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
