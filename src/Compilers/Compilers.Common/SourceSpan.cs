using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers
{
    public class SourceSpan
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
    }

}
