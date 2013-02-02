using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.Generator
{
    public class ReduceAction
    {
        private IProduction m_reduceProduction;
        private IProduction m_reduceTerminal;

        internal ReduceAction(IProduction reduceTerminl, IProduction reduceProduction)
        {
            this.m_reduceTerminal = reduceTerminl;
            this.m_reduceProduction = reduceProduction;
        }

        public IProduction ReduceTerminal
        {
            get
            {
                return m_reduceTerminal;
            }
        }

        public IProduction ReduceProduction
        {
            get
            {
                return m_reduceProduction;
            }
        }
    }
}
