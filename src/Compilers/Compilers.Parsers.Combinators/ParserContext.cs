using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ParserContext
    {
        //private int m_trimmingThreshold = Int32.MaxValue;
        //private long m_failedSteps = 0L;
        //private long m_succeedSteps = 0L;

        public CompilationErrorManager ErrorManager { get; set; }
        public int InsertionErrorId { get; set; }
        public int DeletionErrorId { get; set; }

        public ParserContext(CompilationErrorManager errorManager, int deletionErrorId, int insertionErrorId)
        {
            ErrorManager = errorManager;
            InsertionErrorId = insertionErrorId;
            DeletionErrorId = deletionErrorId;
        }

        public ParserContext()
            : this(null, 0, 0) { }

        public void DefineDefaultCompilationErrorInfo(int errorLevel)
        {
            if (ErrorManager == null)
            {
                throw new InvalidOperationException("ErrorManager is not specified");
            }

            ErrorManager.DefineError(DeletionErrorId, errorLevel, CompilationStage.Parsing, "Unexpected token \"{0}\"");
            ErrorManager.DefineError(InsertionErrorId, errorLevel, CompilationStage.Parsing, "Missing {0}");
        }

        //public void ResetFailedStepCount()
        //{
        //    m_failedSteps = 0L;
        //    m_succeedSteps = 0L;
        //    m_trimmingThreshold = Int32.MaxValue;
        //}

        public Result<T> StepResult<T>(int cost, Func<Result<T>> nextResult)
        {
            //if (cost > 0)
            //{
            //    m_failedSteps++;
            //}
            //else
            //{
            //    m_succeedSteps++;

            //    if (m_failedSteps < m_succeedSteps * 6)
            //    {
            //        m_trimmingThreshold = Int32.MaxValue;
            //    }
            //    else if (m_failedSteps < m_succeedSteps * 10)
            //    {
            //        m_trimmingThreshold = 1;
            //    }
            //}
            return new StepResult<T>(cost, nextResult, null);
        }

        public Result<T> StepResult<T>(int cost, Func<Result<T>> nextResult, ErrorCorrection errorCorrection)
        {
            //if (cost > 0)
            //{
            //    m_failedSteps++;

            //    if (m_failedSteps >= m_succeedSteps * 10)
            //    {
            //        m_trimmingThreshold = 0;
            //    }
            //    else if (m_failedSteps >= m_succeedSteps * 6)
            //    {
            //        m_trimmingThreshold = 1;
            //    }
            //}
            //else
            //{
            //    m_succeedSteps++;
            //}

            return new StepResult<T>(cost, nextResult, errorCorrection);
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
                //if (correctionDepth >= m_trimmingThreshold)
                //{
                //    if (step1.ErrorCorrection != null && step1.ErrorCorrection.Method == CorrectionMethod.Inserted)
                //    {
                //        return step2;
                //    }
                //}

                return new StepResult<T>(Math.Max(step1.Cost, step2.Cost), 
                    () => ChooseBest(step1.NextResult, step2.NextResult, correctionDepth + 1), null);
            }
        }
    }
}
