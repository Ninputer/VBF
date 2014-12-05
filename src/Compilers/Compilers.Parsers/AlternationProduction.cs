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
    public class AlternationProduction<T> : ProductionBase<T>
    {
        public AlternationProduction(ProductionBase<T> production1, ProductionBase<T> production2)
        {
            CodeContract.RequiresArgumentNotNull(production1, "production1");
            CodeContract.RequiresArgumentNotNull(production2, "production2");

            Production1 = production1;
            Production2 = production2;
        }

        public ProductionBase<T> Production1 { get; private set; }
        public ProductionBase<T> Production2 { get; private set; }

        public override string DebugName
        {
            get
            {
                return "A" + DebugNameSuffix;
            }
        }

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg arg)
        {
            return visitor.VisitAlternation(this, arg);
        }

        public override string ToString()
        {
            return String.Format("{0} ::= {1} | {2}", DebugName, Production1.DebugName, Production2.DebugName);
        }
    }
}
