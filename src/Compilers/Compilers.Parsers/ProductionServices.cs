using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{

    internal class ProductionAggregationService : IProductionVisitor
    {
        public ISet<IProduction> Productions { get; private set; }

        public ProductionAggregationService()
        {
            Productions = new HashSet<IProduction>();
        }

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            Productions.Add(terminal);

            if (terminal.Info == null)
            {
                terminal.Info = new ProductionInfo();

                terminal.Info.First.Add(terminal);
            }
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            if (Productions.Contains(mappingProduction))
            {
                return;
            }

            Productions.Add(mappingProduction);
            mappingProduction.SourceProduction.Accept(this);


            if (mappingProduction.Info == null)
            {
                mappingProduction.Info = new ProductionInfo();
            }
        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
            Productions.Add(endOfStream);

            if (endOfStream.Info == null)
            {
                endOfStream.Info = new ProductionInfo();

                endOfStream.Info.First.Add(endOfStream);
            }
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            Productions.Add(emptyProduction);

            if (emptyProduction.Info == null)
            {
                emptyProduction.Info = new ProductionInfo();

                emptyProduction.Info.IsNullable = true;
            }
        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {
            if (Productions.Contains(alternationProduction))
            {
                return;
            }

            Productions.Add(alternationProduction);

            alternationProduction.Production1.Accept(this);
            alternationProduction.Production2.Accept(this);

            if (alternationProduction.Info == null)
            {
                alternationProduction.Info = new ProductionInfo();
            }
        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {
            if (Productions.Contains(concatenationProduction))
            {
                return;
            }

            Productions.Add(concatenationProduction);

            concatenationProduction.ProductionLeft.Accept(this);
            concatenationProduction.ProductionRight.Accept(this);

            if (concatenationProduction.Info == null)
            {
                concatenationProduction.Info = new ProductionInfo();
            }
        }
    }

    public class ProductionInfoServices : IProductionVisitor
    {

        private bool IsChanged;

        public void Visit(IProduction production)
        {

            var aggregator = new ProductionAggregationService();
            production.Accept(aggregator);


            do
            {
                IsChanged = false;

                foreach (var p in aggregator.Productions)
                {
                    p.Accept(this);
                }

            } while (IsChanged);
        }

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            var source = mappingProduction.SourceProduction;

            IsChanged = ProductionInfo.UnionSet(mappingProduction.Info.First, source.Info.First) || IsChanged;
            IsChanged = ProductionInfo.UnionSet(source.Info.Follow, mappingProduction.Info.Follow) || IsChanged;
        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
           
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            
        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {            
            var info = alternationProduction.Info;

            var info1 = alternationProduction.Production1.Info;
            var info2 = alternationProduction.Production2.Info;


            IsChanged = ProductionInfo.UnionSet(info.First, info1.First) || IsChanged;
            IsChanged = ProductionInfo.UnionSet(info.First, info2.First) || IsChanged;

            IsChanged = ProductionInfo.UnionSet(info1.Follow, info.Follow) || IsChanged;
            IsChanged = ProductionInfo.UnionSet(info2.Follow, info.Follow) || IsChanged;

            bool isNullable = info1.IsNullable || info2.IsNullable;

            if (info.IsNullable != isNullable)
            {
                IsChanged = true;
                info.IsNullable = isNullable;
            }
        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {
            var p1 = concatenationProduction.ProductionLeft;
            var p2 = concatenationProduction.ProductionRight;

            var info = concatenationProduction.Info;
            var info1 = p1.Info;
            var info2 = p2.Info;

            IsChanged = ProductionInfo.UnionSet(info.First, info1.First) || IsChanged;

            if (info1.IsNullable)
            {
                IsChanged = ProductionInfo.UnionSet(info.First, info2.First) || IsChanged;
            }

            if (info2.IsNullable)
            {
                IsChanged = ProductionInfo.UnionSet(info1.Follow, info.Follow) || IsChanged;
            }

            IsChanged = ProductionInfo.UnionSet(info2.Follow, info.Follow) || IsChanged;
            IsChanged = ProductionInfo.UnionSet(info1.Follow, info2.First) || IsChanged;

            bool isNullable = info1.IsNullable && info2.IsNullable;

            if (info.IsNullable != isNullable)
            {
                IsChanged = true;
                info.IsNullable = isNullable;
            }

        }
    }
}
