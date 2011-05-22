using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class ArrayLookup : Expression
    {
        public Expression Array { get; set; }
        public Expression Index { get; private set; }

        public ArrayLookup(Expression array, Expression index)
        {
            Array = array;
            Index = index;
        }
    }
}
