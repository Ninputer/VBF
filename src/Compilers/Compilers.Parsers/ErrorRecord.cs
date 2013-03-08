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

        public override bool Equals(object obj)
        {
            ErrorRecord other = obj as ErrorRecord;
            if (other == null)
            {
                return false;
            }

            return ErrorId == other.ErrorId &&
                Object.Equals(ErrorPosition, other.ErrorPosition) &&
                Object.Equals(ErrorArgument, other.ErrorArgument);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ErrorId ?? -1) ^ ErrorPosition.GetHashCode();
            }
        }
    }
}
