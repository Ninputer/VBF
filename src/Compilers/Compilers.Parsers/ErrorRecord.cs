using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    internal class ErrorRecord
    {
        public int? ErrorId;
        public SourceSpan ErrorPosition;
        public Object ErrorArgument;

        public ErrorRecord(int? id, SourceSpan position)
        {
            ErrorId = id;
            ErrorPosition = position;
        }
    }
}
