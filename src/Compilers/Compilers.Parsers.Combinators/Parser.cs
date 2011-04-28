using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public delegate Result<T> Parser<T>(ForkableScanner scanner);

    public class Result<T>
    {
        public T Value { get; private set; }

        public Result(T value)
        {
            Value = value;
        }
    }
}
