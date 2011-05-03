using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers
{
    static class CodeContract
    {
        public static void RequiresArgumentNotNull(object argValue, string argName)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        public static void RequiresArgumentNotNull(object argValue, string argName, string message)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName, message);
            }
        }

        public static void RequiresArgumentInRange(bool isInRange, string argName, string message)
        {
            if (!isInRange)
            {
                throw new ArgumentOutOfRangeException(argName, message);
            }
        }

        public static void Requires(bool condition, string argName)
        {
            if (!condition)
            {
                throw new ArgumentException(argName);
            }
        }

        public static void Requires(bool condition, string argName, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(argName, message);
            }
        }
    }
}
