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

namespace VBF.Compilers.Parsers
{
    public class MappingProduction<TSource, TReturn> : ProductionBase<TReturn>
    {
        public MappingProduction(ProductionBase<TSource> sourceProduction, Func<TSource, TReturn> selector, Func<TReturn, bool> validationRule, int? errorId, Func<TReturn, SourceSpan> positionGetter)
        {
            CodeContract.RequiresArgumentNotNull(sourceProduction, "sourceProduction");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            SourceProduction = sourceProduction;
            Selector = selector;
            ValidationRule = validationRule;
            ValidationErrorId = errorId;
            PositionGetter = positionGetter;
        }

        public MappingProduction(ProductionBase<TSource> sourceProduction, Func<TSource, TReturn> selector) : this(sourceProduction, selector, null, null, null) { }
        public ProductionBase<TSource> SourceProduction { get; private set; }
        public Func<TSource, TReturn> Selector { get; private set; }
        public Func<TReturn, bool> ValidationRule { get; private set; }
        public int? ValidationErrorId { get; private set; }
        public Func<TReturn, SourceSpan> PositionGetter { get; private set; }

        public override string DebugName
        {
            get
            {
                return "M" + DebugNameSuffix;
            }
        }

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument)
        {
            return visitor.VisitMapping(this, argument);
        }

        public override string ToString()
        {
            return String.Format("{0} ::= {1}", DebugName, SourceProduction.DebugName);
        }
    }
}
