using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class ArrayLength : Expression
    {
        public Expression Array { get; private set; }

        public ArrayLength(Expression array)
        {
            Array = array;
        }
    }
}
