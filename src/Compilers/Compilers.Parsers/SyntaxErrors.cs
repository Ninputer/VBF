using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    public class SyntaxErrors
    {
        public int TokenMissing { get; set; }
        public int TokenUnexpected { get; set; }
        public int OtherError { get; set; }
    }
}
