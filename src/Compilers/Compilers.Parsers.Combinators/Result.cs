using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers.Combinators
{
    public enum ResultType
    {
        Step,
        Stop
    }

    public abstract class Result<T>
    {
        public List<ErrorCorrection> ErrorCorrections { get; private set; }
        public ResultType Type { get; private set; }
        public abstract T GetResult(ParserContext context);

        internal Result(ResultType type)
        {
            Type = type;
            ErrorCorrections = new List<ErrorCorrection>();
        }
    }

    public sealed class StepResult<T> : Result<T>
    {
        private Func<Result<T>> m_nextResultFuture;
        private Result<T> m_nextResult;
        public int Cost { get; private set; }
        public ErrorCorrection ErrorCorrection { get; private set; }

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

        internal void SetNextResult(Result<T> nextResult)
        {
            m_nextResult = nextResult;
        }

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

        public override T GetResult(ParserContext context)
        {
            return NextResult.GetResult(context);
        }
    }

    public sealed class StopResult<T> : Result<T>
    {
        public T Result { get; private set; }
        internal StopResult(T result)
            : base(ResultType.Stop)
        {
            Result = result;
        }

        public override T GetResult(ParserContext context)
        {
            if (context != null && context.ErrorManager != null)
            {
                foreach (var ec in ErrorCorrections)
                {
                    if (ec.Method == CorrectionMethod.Inserted)
                    {
                        context.ErrorManager.AddError(
                            context.InsertionErrorId, ec.CorrectionLexeme.Span, ec.CorrectionToken);
                    }
                    else
                    {
                        context.ErrorManager.AddError(
                            context.DeletionErrorId, ec.CorrectionLexeme.Span, ec.CorrectionLexeme.Value);
                    }
                }
            }
            return Result;
        }
    }
}
