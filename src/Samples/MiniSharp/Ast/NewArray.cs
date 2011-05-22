using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class NewArray
    {
        public Expression Length { get; private set; }

        public NewArray(Expression length)
        {
            Length = length;
        }
    }
}
