using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ParserContext
    {
        private readonly int m_trimmingThreshold;
        public CompilationErrorManager ErrorManager { get; set; }
        public int InsertionErrorId { get; set; }
        public int DeletionErrorId { get; set; }

        public ParserContext(CompilationErrorManager errorManager, int deletionErrorId, int insertionErrorId, int errorCorrectionTrimmingThreshold)
        {
            ErrorManager = errorManager;
            InsertionErrorId = insertionErrorId;
            DeletionErrorId = deletionErrorId;

            m_trimmingThreshold = errorCorrectionTrimmingThreshold;
        }

        public ParserContext(CompilationErrorManager errorManager, int deletionErrorId, int insertionErrorId)
            : this(errorManager, deletionErrorId, insertionErrorId, 1) { }

        public ParserContext(int maxInsertionCorrectionDepth)
            : this(null, 0, 0, maxInsertionCorrectionDepth) { }

        public ParserContext()
            : this(null, 0, 0, 1) { }

        public void DefineDefaultCompilationErrorInfo(int errorLevel)
        {
            if (ErrorManager == null)
            {
                throw new InvalidOperationException("ErrorManager is not specified");
            }

            ErrorManager.DefineError(DeletionErrorId, errorLevel, CompilationStage.Parsing, "Unexpected token \"{0}\"");
            ErrorManager.DefineError(InsertionErrorId, errorLevel, CompilationStage.Parsing, "Missing token of {0}");
        }

        public Result<T> StepResult<T>(int cost, Func<Result<T>> nextResult)
        {
            return new StepResult<T>(cost, nextResult, null);
        }

        public Result<T> StepResult<T>(int cost, Func<Result<T>> nextResult, ErrorCorrection errorCorrection)
        {
            return new StepResult<T>(cost, nextResult, errorCorrection);
        }

        public Result<T> FailResult<T>()
        {
            var failResult = new StepResult<T>(1);
            failResult.SetNextResult(failResult);

            return failResult;
        }

        public Result<T> StopResult<T>(T result)
        {
            return new StopResult<T>(result);
        }

        public Result<T> ChooseBest<T>(Result<T> result1, Result<T> result2)
        {
            return ChooseBest(result1, result2, 0);
        }

        private Result<T> ChooseBest<T>(Result<T> result1, Result<T> result2, int correctionDepth)
        {
            if (result1.Type == ResultType.Stop)
            {
                return result1;
            }
            if (result2.Type == ResultType.Stop)
            {
                return result2;
            }

            var step1 = (StepResult<T>)result1;
            var step2 = (StepResult<T>)result2;

            if (step1.Cost < step2.Cost)
            {
                return step1;
            }
            else if (step1.Cost > step2.Cost)
            {
                return step2;
            }
            else
            {
                if (correctionDepth >= m_trimmingThreshold)
                {
                    if (step1.ErrorCorrection != null && step1.ErrorCorrection.Method == CorrectionMethod.Inserted)
                    {
                        return step2;
                    }
                    else if (step2.ErrorCorrection != null && step2.ErrorCorrection.Method == CorrectionMethod.Inserted)
                    {
                        return step1;
                    }
                }
                else if (correctionDepth > 10)
                {
                    return FailResult<T>();
                }

                return new StepResult<T>(1, () => ChooseBest(step1.NextResult, step2.NextResult, correctionDepth + 1), null);
            }
        }
    }
}
