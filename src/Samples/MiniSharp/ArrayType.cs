using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp
{
    public class ArrayType : TypeBase
    {
        public TypeBase ElementType { get; set; }

        public static readonly ArrayType IntArray = new ArrayType() { Name = "int[]", ElementType = PrimaryType.Int };
    }
}
