using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContinuationParserCombinators
{
    public class Result<T>
    {
    }

    public class Step<T> : Result<T>
    {
        public Step(int cost, Result<T> nextResult)
        {

        }
    }

    public class Stop<T> :Result<T>
    {
        public Stop(T result)
        {

        }
    }
}
