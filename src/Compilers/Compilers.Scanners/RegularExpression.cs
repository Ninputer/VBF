using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// The base class of regular expressions. Provides methods to build a regular expression
    /// </summary>
    public abstract class RegularExpression
    {
        public RegularExpressionType ExpressionType { get; private set; }

        protected RegularExpression(RegularExpressionType expType)
        {
            ExpressionType = expType;
        }
    }
}
