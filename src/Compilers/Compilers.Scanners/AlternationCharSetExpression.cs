using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.Compilers.Scanners
{
    public sealed class AlternationCharSetExpression : RegularExpression
    {
        private List<char> m_charSet;

        public AlternationCharSetExpression(IEnumerable<char> charset)
            : base(RegularExpressionType.AlternationCharSet)
        {
            m_charSet = new List<char>(charset);
        }

        public new ReadOnlyCollection<char> CharSet
        {
            get
            {
                return m_charSet.AsReadOnly();
            }
        }

    }
}
