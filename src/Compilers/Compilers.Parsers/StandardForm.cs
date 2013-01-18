using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    /// <summary>
    /// Standard form of a production rule. Used internally
    /// </summary>
    public class StandardForm
    {
        public int ComponentCount
        {
            get
            {
                return 0;
            }
        }

        public int this[int index]
        {
            get
            {
                return 0;
            }
        }

        public IReadOnlyCollection<Token> First
        {
            get
            {
                return null;
            }
        }

        public IReadOnlyCollection<Token> Follow
        {
            get
            {
                return null;
            }
        }

        public bool IsNullable { get; private set; }
    }
}
