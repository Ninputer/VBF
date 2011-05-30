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
    }
}
