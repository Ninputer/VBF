using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    internal class ProductionAggregationVisitor : IProductionVisitor
    {
        public List<IProduction> Productions { get; private set; }

        public ProductionAggregationVisitor()
        {
            Productions = new List<IProduction>();
        }

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            if (terminal.Info != null)
            {
                return;
            }

            terminal.Info = new ProductionInfo();
            terminal.Info.First.Add(terminal);

            terminal.Info.Index = Productions.Count;
            Productions.Add(terminal);
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            if (mappingProduction.Info != null)
            {
                return;
            }

            mappingProduction.Info = new ProductionInfo();
            mappingProduction.SourceProduction.Accept(this);

            mappingProduction.Info.Index = Productions.Count;
            Productions.Add(mappingProduction);
        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
            if (endOfStream.Info != null)
            {
                return;
            }

            endOfStream.Info = new ProductionInfo();
            endOfStream.Info.First.Add(endOfStream);

            endOfStream.Info.Index = Productions.Count;
            Productions.Add(endOfStream);
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            if (emptyProduction.Info != null)
            {
                return;
            }

            emptyProduction.Info = new ProductionInfo();
            emptyProduction.Info.IsNullable = true;

            emptyProduction.Info.Index = Productions.Count;
            Productions.Add(emptyProduction);
        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {
            if (alternationProduction.Info != null)
            {
                return;
            }

            alternationProduction.Info = new ProductionInfo();

            alternationProduction.Production1.Accept(this);
            alternationProduction.Production2.Accept(this);

            alternationProduction.Info.Index = Productions.Count;
            Productions.Add(alternationProduction);
        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {
            if (concatenationProduction.Info != null)
            {
                return;
            }

            concatenationProduction.Info = new ProductionInfo();

            concatenationProduction.ProductionLeft.Accept(this);
            concatenationProduction.ProductionRight.Accept(this);

            concatenationProduction.Info.Index = Productions.Count;
            Productions.Add(concatenationProduction);
        }
    }

    internal class FirstFollowVisitor : IProductionVisitor
    {

        public bool IsChanged { get; set; }

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {

        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            var source = mappingProduction.SourceProduction;

            IsChanged = UnionSet(mappingProduction.Info.First, source.Info.First) || IsChanged;
            IsChanged = UnionSet(source.Info.Follow, mappingProduction.Info.Follow) || IsChanged;
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


            IsChanged = UnionSet(info.First, info1.First) || IsChanged;
            IsChanged = UnionSet(info.First, info2.First) || IsChanged;

            IsChanged = UnionSet(info1.Follow, info.Follow) || IsChanged;
            IsChanged = UnionSet(info2.Follow, info.Follow) || IsChanged;

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

            IsChanged = UnionSet(info.First, info1.First) || IsChanged;

            if (info1.IsNullable)
            {
                IsChanged = UnionSet(info.First, info2.First) || IsChanged;
            }

            if (info2.IsNullable)
            {
                IsChanged = UnionSet(info1.Follow, info.Follow) || IsChanged;
            }

            IsChanged = UnionSet(info2.Follow, info.Follow) || IsChanged;
            IsChanged = UnionSet(info1.Follow, info2.First) || IsChanged;

            bool isNullable = info1.IsNullable && info2.IsNullable;

            if (info.IsNullable != isNullable)
            {
                IsChanged = true;
                info.IsNullable = isNullable;
            }

        }

        internal static bool UnionSet<T>(ISet<T> set, IEnumerable<T> toUnion)
        {
            bool changed = false;

            foreach (var item in toUnion)
            {
                changed = set.Add(item) || changed;
            }

            return changed;
        }
    }

    internal class ClosureVisitor : IProductionVisitor
    {
        public ISet<int> Closure { get; private set; }

        public ClosureVisitor()
        {
            Closure = new SortedSet<int>();
        }

        public void Reset()
        {
            Closure.Clear();
        }

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            throw new NotSupportedException("Terminal production is not supported");
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {

        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
            throw new NotSupportedException("End of stream production is not supported");
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {

        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {

        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {

        }
    }
}
