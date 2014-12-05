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
        protected ErrorCorrection(CorrectionMethod method)
        {
            Method = method;
        }

        public CorrectionMethod Method { get; private set; }

        public abstract void AddError(ParserContext context);
    }

    public class InsertedErrorCorrection : ErrorCorrection
    {
        public InsertedErrorCorrection(string correctionToken, SourceSpan insertPoint)
            : base(CorrectionMethod.Inserted)
        {
            CorrectionToken = correctionToken;
            InsertPoint = insertPoint;
        }

        public string CorrectionToken { get; private set; }
        public SourceSpan InsertPoint { get; private set; }

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
        public DeletedErrorCorrection(Lexeme unexpectedLexeme)
            : base(CorrectionMethod.Deleted)
        {
            UnexpectedLexeme = unexpectedLexeme;
        }

        public Lexeme UnexpectedLexeme { get; private set; }

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
        public CustomErrorCorrection(int errorId, SourceSpan errorSpan, params object[] parameters)
            : base(CorrectionMethod.Custom)
        {
            ErrorId = errorId;
            ErrorSpan = errorSpan;
            Parameters = parameters;
        }

        public int ErrorId { get; private set; }
        public SourceSpan ErrorSpan { get; private set; }
        public Object[] Parameters { get; private set; }

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
