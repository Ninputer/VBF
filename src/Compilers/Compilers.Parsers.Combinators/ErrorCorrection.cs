using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public enum CorrectionMethod
    {
        Inserted,
        Deleted,
        Custom
    }

    public abstract class ErrorCorrection
    {
        public CorrectionMethod Method { get; private set; }

        protected ErrorCorrection(CorrectionMethod method)
        {
            Method = method;
        }

        public abstract void AddError(ParserContext context);
    }

    public class InsertedErrorCorrection : ErrorCorrection
    {
        public string CorrectionToken { get; private set; }
        public SourceSpan InsertPoint { get; private set; }

        public InsertedErrorCorrection(string correctionToken, SourceSpan insertPoint)
            : base(CorrectionMethod.Inserted)
        {
            CorrectionToken = correctionToken;
            InsertPoint = insertPoint;
        }

        public override void AddError(ParserContext context)
        {
            if (context != null && context.ErrorList != null)
            {
                context.ErrorList.AddError(
                    context.InsertionErrorId, InsertPoint, CorrectionToken);
            }
        }
    }

    public class DeletedErrorCorrection : ErrorCorrection
    {
        public Lexeme UnexpectedLexeme { get; private set; }

        public DeletedErrorCorrection(Lexeme unexpectedLexeme)
            : base(CorrectionMethod.Deleted)
        {
            UnexpectedLexeme = unexpectedLexeme;
        }

        public override void AddError(ParserContext context)
        {
            if (context != null && context.ErrorList != null)
            {
                context.ErrorList.AddError(
                    context.DeletionErrorId, UnexpectedLexeme.Value.Span, UnexpectedLexeme.Value);
            }
        }
    }

    public class CustomErrorCorrection : ErrorCorrection
    {
        public int ErrorId { get; private set; }
        public SourceSpan ErrorSpan { get; private set; }
        public Object[] Parameters { get; private set; }

        public CustomErrorCorrection(int errorId, SourceSpan errorSpan, params object[] parameters)
            : base(CorrectionMethod.Custom)
        {
            ErrorId = errorId;
            ErrorSpan = errorSpan;
            Parameters = parameters;
        }

        public override void AddError(ParserContext context)
        {
            if (context != null && context.ErrorList != null)
            {
                context.ErrorList.AddError(
                    ErrorId, ErrorSpan, Parameters);
            }
        }
    }
}
