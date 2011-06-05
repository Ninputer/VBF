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
        public static readonly ArrayType StrArray = new ArrayType() { Name = "string[]", ElementType = PrimaryType.String };

        public override bool IsAssignableFrom(TypeBase type)
        {
            CodeClassType elementClassType = ElementType as CodeClassType;
            ArrayType arrayType = type as ArrayType;

            if (elementClassType != null && arrayType != null)
            {
                return elementClassType.IsAssignableFrom(arrayType.ElementType);
            }

            return false;
        }
    }
}
