using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    public class ProductionInfo
    {
        public ISet<IProduction> First { get; private set; }
        public ISet<IProduction> Follow { get; private set; }
        public bool IsNullable { get; set; }

        internal int Index { get; set; }
        internal int DotPosition { get; set; }

        public ProductionInfo()
        {
            First = new HashSet<IProduction>();
            Follow = new HashSet<IProduction>();
            IsNullable = false;
        }        
    }
    
}
