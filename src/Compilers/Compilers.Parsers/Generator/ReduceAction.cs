using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.Generator
{
    public class ReduceAction
    {
        private IProduction reduceProduction;
        private IProduction reduceSymbol;
        

        public ReduceAction(IProduction reduceSymbol, IProduction reduceProduction)
        {
            // TODO: Complete member initialization
            this.reduceSymbol = reduceSymbol;
            this.reduceProduction = reduceProduction;
        }
    }
}
