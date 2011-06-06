using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp
{
    public class PrimaryType : TypeBase
    {
        public static readonly PrimaryType Int = new PrimaryType() { Name = "int" };
        public static readonly PrimaryType Boolean = new PrimaryType() { Name = "bool" };
        public static readonly PrimaryType String = new PrimaryType() { Name = "string" };
        public static readonly PrimaryType Void = new PrimaryType() { Name = "void" };

        public static readonly PrimaryType Unknown = new PrimaryType() { Name = null };

        public override bool IsAssignableFrom(TypeBase type)
        {
            if (this == type)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
