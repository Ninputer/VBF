using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers
{
    public class SourceSpan : IEquatable<SourceSpan>
    {

        private readonly SourceLocation m_startLocation;
        public SourceLocation StartLocation
        {
            get { return m_startLocation; }
        }

        private readonly SourceLocation m_endLocation;
        public SourceLocation EndLocation
        {
            get { return m_endLocation; }
        }

        public SourceSpan(SourceLocation startLocation, SourceLocation endLocation)
        {
            m_startLocation = startLocation;
            m_endLocation = endLocation;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SourceSpan);
        }

        public override int GetHashCode()
        {
            return (m_endLocation.GetHashCode() << 16) ^ m_startLocation.GetHashCode();
        }

        public bool Equals(SourceSpan other)
        {
            if (other == null)
            {
                return false;
            }

            return m_startLocation.Equals(other.m_startLocation) &&
                m_endLocation.Equals(other.m_endLocation);
        }
    }

}
