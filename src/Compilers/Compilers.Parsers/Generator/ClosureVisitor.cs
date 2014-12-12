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

using System.Collections.Generic;

namespace VBF.Compilers.Parsers.Generator
{
    struct ClosureInfo
    {
        public int DotLocation;
        public bool IsChanged;
        public ISet<LR0Item> LR0ItemSet;

        public ClosureInfo(int dotLocation, bool isChanged, ISet<LR0Item> itemSet)
        {
            DotLocation = dotLocation;
            IsChanged = isChanged;
            LR0ItemSet = itemSet;
        }
    }

    internal class ClosureVisitor : IProductionVisitor<ClosureInfo, bool>
    {

        //public bool IsChanged { get; set; }
        //public int DotLocation { get; set; }

        //public ISet<LR0Item> LR0ItemSet { get; set; }

        bool IProductionVisitor<ClosureInfo, bool>.VisitTerminal(Terminal terminal, ClosureInfo arg)
        {
            //do nothing, make set unchanged
            return arg.IsChanged;
        }

        bool IProductionVisitor<ClosureInfo, bool>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, ClosureInfo arg)
        {
            if (arg.DotLocation == 0 && !mappingProduction.SourceProduction.IsTerminal)
            {
                arg.IsChanged = arg.LR0ItemSet.Add(new LR0Item(mappingProduction.SourceProduction.Info.Index, 0)) || arg.IsChanged;
            }

            return arg.IsChanged;
        }

        bool IProductionVisitor<ClosureInfo, bool>.VisitEndOfStream(EndOfStream endOfStream, ClosureInfo arg)
        {
            //do nothing, make set unchanged
            return arg.IsChanged;
        }

        bool IProductionVisitor<ClosureInfo, bool>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, ClosureInfo arg)
        {
            //do nothing, make set unchanged
            return arg.IsChanged;
        }

        bool IProductionVisitor<ClosureInfo, bool>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, ClosureInfo arg)
        {
            if (arg.DotLocation == 0)
            {
                if (!alternationProduction.Production1.IsTerminal)
                {
                   arg.IsChanged = arg.LR0ItemSet.Add(new LR0Item(alternationProduction.Production1.Info.Index, 0)) || arg.IsChanged;
                }

                if (!alternationProduction.Production2.IsTerminal)
                {
                    arg.IsChanged = arg.LR0ItemSet.Add(new LR0Item(alternationProduction.Production2.Info.Index, 0)) || arg.IsChanged;
                }
            }

            return arg.IsChanged;
        }

        bool IProductionVisitor<ClosureInfo, bool>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, ClosureInfo arg)
        {

            switch (arg.DotLocation)
            {
                case 0:
                    if (!concatenationProduction.ProductionLeft.IsTerminal)
                    {
                        arg.IsChanged = arg.LR0ItemSet.Add(new LR0Item(concatenationProduction.ProductionLeft.Info.Index, 0)) || arg.IsChanged;
                    }
                    break;
                case 1:
                    if (!concatenationProduction.ProductionRight.IsTerminal)
                    {
                        arg.IsChanged = arg.LR0ItemSet.Add(new LR0Item(concatenationProduction.ProductionRight.Info.Index, 0)) || arg.IsChanged;
                    }
                    break;
                default:
                    //no symbol at position                    
                    break;
            }

            return arg.IsChanged;
        }
    }
}
