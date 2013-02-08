using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    public class SyntaxErrors
    {
        public int TokenMissingId { get; set; }
        public int TokenUnexpectedId { get; set; }
        public int OtherErrorId { get; set; }
        public int LexicalErrorId { get; set; }
    }
}
