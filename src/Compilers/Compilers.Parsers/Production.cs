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
using VBF.Compilers.Parsers.Generator;

namespace VBF.Compilers.Parsers
{
    public class Production<T> : ProductionBase<T>
    {
        private ProductionBase<T> m_rule;
        public ProductionBase<T> Rule
        {
            get
            {
                if (m_rule == null)
                {
                    throw new InvalidOperationException("The Rule property of this Production is not set. Please set before parsing.");
                }

                return m_rule;
            }
            set
            {
                CodeContract.RequiresArgumentNotNull(value, "value");
                m_rule = value;
            }
        }

        internal override ProductionInfo Info
        {
            get
            {
                return Rule.Info;
            }
            set
            {
                Rule.Info = value;
            }
        }

        public override bool IsTerminal
        {
            get
            {
                return Rule.IsTerminal;
            }
        }

        public override bool IsEos
        {
            get
            {
                return Rule.IsEos;
            }
        }

        public override string DebugName
        {
            get
            {
                return Rule.DebugName;
            }
        }

        public override Func<T, T, T> AmbiguityAggregator
        {
            get
            {
                return Rule.AmbiguityAggregator;
            }
            set
            {
                Rule.AmbiguityAggregator = value;
            }
        }

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument)
        {
            return Rule.Accept(visitor, argument);
        }

        public override bool Equals(object obj)
        {
            return Rule.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Rule.GetHashCode();
        }

        public override string ToString()
        {
            return Rule.ToString();
        }
    }
}
