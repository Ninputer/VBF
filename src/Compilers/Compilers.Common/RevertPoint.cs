using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers
{
    public struct RevertPoint
    {
        internal int Offset { get; private set; }
        internal int Key { get; private set; }
        public SourceLocation LastLocation { get; private set; }
        internal SourceLocation Location { get; private set; }

        internal RevertPoint(int key, int offset, SourceLocation lastLocation, SourceLocation location)
            : this()
        {
            Key = key;
            Offset = offset;
            LastLocation = lastLocation;
            Location = location;
        }
    }
}
