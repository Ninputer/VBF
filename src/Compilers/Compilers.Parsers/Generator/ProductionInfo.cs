using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.Generator
{
    public class ProductionInfo
    {
        public ISet<IProduction> First { get; private set; }
        public ISet<IProduction> Follow { get; private set; }
        public bool IsNullable { get; internal set; }

        internal int Index { get; set; }
        internal int SymbolCount { get; set; }
        internal int NonTerminalIndex { get; set; }

        public ProductionInfo()
        {
            First = new HashSet<IProduction>();
            Follow = new HashSet<IProduction>();
            IsNullable = false;
        }        
    }
    
}
