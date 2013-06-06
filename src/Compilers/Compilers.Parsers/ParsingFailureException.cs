using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public class ParsingFailureException : Exception
    {
        public ParsingFailureException(string message) :base(message)
        {

        }
    }
}
