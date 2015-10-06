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
using System.Diagnostics;
using System.Globalization;
using VBF.Compilers.Parsers.Generator;

namespace VBF.Compilers.Parsers
{
    [DebuggerDisplay("{ToString()}")]
    public abstract class ProductionBase : IProduction
    {
        protected ProductionBase()
        {

        }

        internal virtual ProductionInfo Info { get; set; }

        public virtual string DebugName
        {
            get
            {
                return "P";
            }
        }

        protected string DebugNameSuffix
        {
            get
            {
                if (Info != null)
                {
                    return Info.Index.ToString(CultureInfo.InvariantCulture);
                }
                return String.Empty;
            }
        }

        public abstract TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument);

        public virtual bool IsTerminal
        {
            get { return false; }
        }

        public virtual bool IsEos
        {
            get { return false; }
        }

        public abstract bool AggregatesAmbiguities { get; }
        internal abstract AmbiguityAggregator CreateAggregator();
        internal abstract object GetDefaultResult();
    }

    public abstract class ProductionBase<T> : ProductionBase
    {
        protected ProductionBase()
        {

        }

        public virtual Func<T, T, T> AmbiguityAggregator { get; set; }

        public sealed override bool AggregatesAmbiguities
        {
            get { return AmbiguityAggregator != null; }
        }

        public static ProductionBase<T> operator |(ProductionBase<T> p1, ProductionBase<T> p2)
        {
            return new AlternationProduction<T>(p1, p2);
        }

        internal sealed override AmbiguityAggregator CreateAggregator()
        {
            return new AmbiguityAggregator<T>(Info.NonTerminalIndex, AmbiguityAggregator);
        }

        internal override object GetDefaultResult()
        {
            return DefaultValueContainer<T>.DefaultValue;
        }
    }
}
