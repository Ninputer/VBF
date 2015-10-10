using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    class DefaultValueContainer<T>
    {
        static DefaultValueContainer()
        {
            if (typeof(T) == typeof(Lexeme))
            {
                DefaultValue = (T)(object)Lexeme.CreateEmptyLexeme();
            }
            else
            {
                DefaultValue = default(T);
            }
        }

        public static readonly T DefaultValue;
    }
}
