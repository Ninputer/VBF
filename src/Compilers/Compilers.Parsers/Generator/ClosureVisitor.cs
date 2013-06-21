using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers.Generator
{
    struct ClosureInfo
    {
        public bool IsChanged;
        public int DotLocation;
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
