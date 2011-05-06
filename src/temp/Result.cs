using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContinuationParserCombinators
{
    public class Result<T>
    {
        
    }

    public class Result
    {
        public static Result<T> Step<T>(int cost, Func<Result<T>> nextResult)
        {
            return new Step<T>(cost, nextResult);
        }

        public static Result<T> Stop<T>(T result)
        {
            return new Stop<T>(result);
        }

        public static Result<T> Fail<T>()
        {
            Step<T> fail = new Step<T>(1, null);
            fail.m_result = fail;

            return fail;
        }
     }

    public class Step<T> : Result<T>
    {
        private Func<Result<T>> m_nextFunc;
        internal Result<T> m_result;
        public int Cost { get; private set; }

        public Step(int cost, Func<Result<T>> nextResult)
        {
            m_nextFunc = nextResult;
            //m_result = NextResult;
            Cost = cost;
        }

        public Result<T> NextResult
        {
            get
            {
                if (m_result == null)
                {
                    m_result = m_nextFunc();
                }

                return m_result;
            }
        }
    }

    public class Stop<T> :Result<T>
    {
        public T Result { get; private set; }
        public Stop(T result)
        {
            Result = result;
        }
    }
}
