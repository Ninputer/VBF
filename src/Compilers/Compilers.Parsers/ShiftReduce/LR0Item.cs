using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.ShiftReduce
{
    public struct LR0Item : IEquatable<LR0Item>
    {
        public int ProductionIndex { get; set; }
        public int ItemLocation { get; set; }

        public LR0Item(int productionIndex, int itemLocation)
        {
            ProductionIndex = productionIndex;
            ItemLocation = itemLocation;
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
            return ProductionIndex == other.ProductionIndex && ItemLocation == other.ItemLocation;
        }

        public override int GetHashCode()
        {
            return (ItemLocation << 16) ^ ProductionIndex;
        }
    }
}
