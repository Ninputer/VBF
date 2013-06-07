using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class ResultInfo
    {
        internal ResultInfo(int errorCount)
        {
            ErrorCount = errorCount;
        }

        public int ErrorCount { get; private set; }
    }
}
