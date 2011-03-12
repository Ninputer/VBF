using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public enum RegularExpressionType
    {
        Empty,
        Symbol,
        Alternation,
        Concatenation,
        KleeneStar,
        AlternationCharSet,
        StringLiteral
    }
}
