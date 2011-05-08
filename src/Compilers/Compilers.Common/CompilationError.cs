using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers
{
    [DebuggerDisplay("{ToString()}")]
    public class CompilationError
    {
        public CompilationErrorInfo Info { get; private set; }
        public string Message { get; private set; }
        public SourceSpan ErrorPosition { get; private set; }

        public CompilationError(CompilationErrorInfo errorInfo, SourceSpan errorPosition, string errorMessage)
        {
            Info = errorInfo;
            ErrorPosition = errorPosition;
            Message = errorMessage;
        }

        public override string ToString()
        {
            return String.Format("{0} : {1}  Line: {2} Column: {3}", Info.Id, Message, ErrorPosition.StartLocation.Line, ErrorPosition.StartLocation.Column);
        }
    }
}
