// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;

namespace VBF.Compilers
{
    [DebuggerDisplay("{ToString()}")]
    public class CompilationError
    {
        public CompilationError(CompilationErrorInfo errorInfo, SourceSpan errorPosition, string errorMessage)
        {
            Info = errorInfo;
            ErrorPosition = errorPosition;
            Message = errorMessage;
        }

        public CompilationErrorInfo Info { get; private set; }
        public string Message { get; private set; }
        public SourceSpan ErrorPosition { get; private set; }

        public override string ToString()
        {
            return String.Format("{0} : {1}  Line: {2} Column: {3}", Info.Id, Message, ErrorPosition.StartLocation.Line, ErrorPosition.StartLocation.Column);
        }
    }
}
