// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace VBF.Compilers.Parsers.Combinators
{
    public enum ResultType
    {
        Step,
        Stop
    }

    public abstract class Result<T>
    {
        internal Result(ResultType type)
        {
            Type = type;
            ErrorCorrections = new List<ErrorCorrection>();
        }

        public List<ErrorCorrection> ErrorCorrections { get; private set; }
        public ResultType Type { get; private set; }
        public abstract T GetResult(ParserContext context);
    }

    public sealed class StepResult<T> : Result<T>
    {
        private Result<T> m_nextResult;
        private Func<Result<T>> m_nextResultFuture;

        internal StepResult(int cost, Func<Result<T>> nextResultFuture, ErrorCorrection errorCorrection)
            : base(ResultType.Step)
        {
            m_nextResultFuture = nextResultFuture;
            ErrorCorrection = errorCorrection;
            Cost = cost;
        }

        internal StepResult(int cost)
            : base(ResultType.Step)
        {
            Cost = cost;
        }

        public int Cost { get; private set; }
        public ErrorCorrection ErrorCorrection { get; private set; }

        public Result<T> NextResult
        {
            get
            {
                if (m_nextResult == null)
                {
                    m_nextResult = m_nextResultFuture();
                    m_nextResult.ErrorCorrections.AddRange(ErrorCorrections);
                    if (ErrorCorrection != null) m_nextResult.ErrorCorrections.Add(ErrorCorrection);
                }

                return m_nextResult;
            }
        }

        internal void SetNextResult(Result<T> nextResult)
        {
            m_nextResult = nextResult;
        }

        public override T GetResult(ParserContext context)
        {
            return NextResult.GetResult(context);
        }
    }

    public sealed class StopResult<T> : Result<T>
    {
        internal StopResult(T result)
            : base(ResultType.Stop)
        {
            Result = result;
        }

        public T Result { get; private set; }

        public override T GetResult(ParserContext context)
        {
            foreach (var ec in ErrorCorrections)
            {
                ec.AddError(context);
            }
            return Result;
        }
    }
}
