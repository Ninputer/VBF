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

namespace VBF.Compilers
{
    public struct SourceLocation : IEquatable<SourceLocation>, IComparable<SourceLocation>
    {
        public SourceLocation(int index, int line, int column)
            : this()
        {
            CharIndex = index;
            Line = line;
            Column = column;
        }

        public int Line { get; internal set; }
        public int Column { get; internal set; }
        public int CharIndex { get; internal set; }

        public int CompareTo(SourceLocation other)
        {
            var lineDiff = Line - other.Line;
            if (lineDiff > 0)
            {
                return 1;
            }
            if (lineDiff < 0)
            {
                return -1;
            }

            //same line, compare column
            var columnDiff = Column - other.Column;
            if (columnDiff > 0)
            {
                return 1;
            }
            if (columnDiff < 0)
            {
                return -1;

            }
            //same line, same column.
            return 0;
        }

        public bool Equals(SourceLocation other)
        {
            return CharIndex == other.CharIndex && Line == other.Line && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var other = (SourceLocation)obj;

            return Equals(other);
        }

        public override string ToString()
        {
            return string.Format("(Char index: {0}, line: {1}, column: {2})", CharIndex, Line, Column);
        }

        public override int GetHashCode()
        {
            return Line ^ Column << 4 ^ CharIndex << 8;
        }

    }
}
