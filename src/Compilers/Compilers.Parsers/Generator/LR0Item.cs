using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.Generator
{
    public struct LR0Item : IEquatable<LR0Item>
    {
        public int ProductionIndex { get; set; }
        public int DotLocation { get; set; }

        public LR0Item(int productionIndex, int dotLocation) : this()
        {
            ProductionIndex = productionIndex;
            DotLocation = dotLocation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals((LR0Item)obj);
        }

        public bool Equals(LR0Item other)
        {
            return ProductionIndex == other.ProductionIndex && DotLocation == other.DotLocation;
        }

        public override int GetHashCode()
        {
            return (DotLocation << 16) ^ ProductionIndex;
        }
    }
}
