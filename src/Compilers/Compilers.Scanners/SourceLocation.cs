using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public struct SourceLocation : IEquatable<SourceLocation>, IComparable<SourceLocation>
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public int CharIndex { get; set; }

        public SourceLocation(int index, int line, int column)
            : this()
        {
            CharIndex = index;
            Line = line;
            Column = column;
        }

        public int CompareTo(SourceLocation other)
        {
            var lineDiff = Line - other.Line;
            if (lineDiff > 0)
            {
                return 1;
            }
            else if (lineDiff < 0)
            {
                return -1;
            }

            //same line, compare column
            var columnDiff = Column - other.Column;
            if (columnDiff > 0)
            {
                return 1;
            }
            else if (columnDiff < 0)
            {
                return -1;

            }
            else
            {
                //same line, same column.
                return 0;
            }
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
