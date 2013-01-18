using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public interface IProduction
    {
        void Accept(IProductionVisitor visitor);
    }
}
