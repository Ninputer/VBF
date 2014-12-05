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
    public class ConcatenationProduction<T1, T2, TR> : ProductionBase<TR>
    {
        public ConcatenationProduction(ProductionBase<T1> productionLeft, Func<T1, ProductionBase<T2>> productionRightSelector, Func<T1, T2, TR> selector)
        {
            CodeContract.RequiresArgumentNotNull(productionLeft, "productionLeft");
            CodeContract.RequiresArgumentNotNull(productionRightSelector, "productionRightSelector");
            CodeContract.RequiresArgumentNotNull(selector, "selector");

            ProductionLeft = productionLeft;
            ProductionRight = productionRightSelector(default(T1));
            Selector = selector;
        }

        public ProductionBase<T1> ProductionLeft { get; private set; }
        public ProductionBase<T2> ProductionRight { get; private set; }
        public Func<T1, T2, TR> Selector { get; private set; }

        public override string DebugName
        {
            get
            {
                return "C" + DebugNameSuffix;
            }
        }

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument)
        {
            return visitor.VisitConcatenation(this, argument);
        }

        public override string ToString()
        {
            return String.Format("{0} ::= {1} {2}", DebugName, ProductionLeft.DebugName, ProductionRight.DebugName);
        }
    }
}
