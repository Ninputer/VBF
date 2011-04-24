using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public struct RevertPoint
    {
        internal int Offset { get; private set; }
        internal int Key { get; private set; }
        public SourceLocation LastLocation { get; private set; }
        internal SourceLocation Location { get; private set; }
        internal bool IsLastCharLf { get; private set; }

        internal RevertPoint(int key, int offset, SourceLocation lastLocation, SourceLocation location, bool isLastCharLf)
            : this()
        {
            Key = key;
            Offset = offset;
            LastLocation = lastLocation;
            Location = location;
            IsLastCharLf = IsLastCharLf;
        }
    }
}
