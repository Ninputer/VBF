using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public delegate IResult<T> Parser<out T>(ForkableScanner scanner);

    public interface IResult<out T>
    {
        T Value { get; }
    }

    public class Result<T> : IResult<T>
    {
        public T Value { get; private set; }

        public Result(T value)
        {
            Value = value;
        }
    }
}
