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

namespace VBF.Compilers.Parsers
{
    internal class ErrorRecord
    {
        public readonly int? ErrorId;
        public readonly SourceSpan ErrorPosition;
        public Object ErrorArgument;
        public Object ErrorArgument2;

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
                Equals(ErrorPosition, other.ErrorPosition) &&
                Equals(ErrorArgument, other.ErrorArgument) &&
                Equals(ErrorArgument2, other.ErrorArgument2);
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
