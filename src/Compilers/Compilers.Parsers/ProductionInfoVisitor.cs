using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Parsers.Generator;

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
            terminal.Info.SymbolCount = 1;

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
            mappingProduction.Info.SymbolCount = 1;
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
            endOfStream.Info.SymbolCount = 1;

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
            emptyProduction.Info.SymbolCount = 0;
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
            alternationProduction.Info.SymbolCount = 1;
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
            concatenationProduction.Info.SymbolCount = 2;
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

            IsChanged = mappingProduction.Info.First.UnionCheck(source.Info.First) || IsChanged;
            IsChanged = source.Info.Follow.UnionCheck(mappingProduction.Info.Follow) || IsChanged;
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


            IsChanged = info.First.UnionCheck(info1.First) || IsChanged;
            IsChanged = info.First.UnionCheck(info2.First) || IsChanged;

            IsChanged = info1.Follow.UnionCheck(info.Follow) || IsChanged;
            IsChanged = info2.Follow.UnionCheck(info.Follow) || IsChanged;

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

            IsChanged = info.First.UnionCheck(info1.First) || IsChanged;

            if (info1.IsNullable)
            {
                IsChanged = info.First.UnionCheck(info2.First) || IsChanged;
            }

            if (info2.IsNullable)
            {
                IsChanged = info1.Follow.UnionCheck(info.Follow) || IsChanged;
            }

            IsChanged = info2.Follow.UnionCheck(info.Follow) || IsChanged;
            IsChanged = info1.Follow.UnionCheck(info2.First) || IsChanged;

            bool isNullable = info1.IsNullable && info2.IsNullable;

            if (info.IsNullable != isNullable)
            {
                IsChanged = true;
                info.IsNullable = isNullable;
            }

        }


    }

    internal class ClosureVisitor : IProductionVisitor
    {

        public bool IsChanged { get; set; }
        public int DotLocation { get; set; }

        public ISet<LR0Item> LR0ItemSet { get; set; }

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            //do nothing, make set unchanged
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            if (DotLocation == 0 && !mappingProduction.SourceProduction.IsTerminal)
            {
                IsChanged = LR0ItemSet.Add(new LR0Item(mappingProduction.SourceProduction.Info.Index, 0)) || IsChanged;
            }
        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
            //do nothing, make set unchanged
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            //do nothing, make set unchanged
        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {
            if (DotLocation == 0)
            {
                if (!alternationProduction.Production1.IsTerminal)
                {
                    IsChanged = LR0ItemSet.Add(new LR0Item(alternationProduction.Production1.Info.Index, 0)) || IsChanged;
                }

                if (!alternationProduction.Production2.IsTerminal)
                {
                    IsChanged = LR0ItemSet.Add(new LR0Item(alternationProduction.Production2.Info.Index, 0)) || IsChanged;
                }
            }
        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {
            switch (DotLocation)
            {
                case 0:
                    if (!concatenationProduction.ProductionLeft.IsTerminal)
                    {
                        IsChanged = LR0ItemSet.Add(new LR0Item(concatenationProduction.ProductionLeft.Info.Index, 0)) || IsChanged;
                    }
                    break;
                case 1:
                    if (!concatenationProduction.ProductionRight.IsTerminal)
                    {
                        IsChanged = LR0ItemSet.Add(new LR0Item(concatenationProduction.ProductionRight.Info.Index, 0)) || IsChanged;
                    }
                    break;
                default:
                    //no symbol at position                    
                    break;
            }
        }
    }

    internal class DotSymbolVisitor : IProductionVisitor
    {
        public int DotLocation { get; set; }
        public IReadOnlyList<IProduction> Symbols { get; private set; }

        private static readonly IProduction[] s_empty = new IProduction[0];

        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            throw new InvalidOperationException("Terminals are not allowed in LR states");
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            if (DotLocation == 0)
            {
                Symbols = new IProduction[1] { mappingProduction.SourceProduction };
            }
            else
            {
                Symbols = s_empty;
            }
        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
            throw new InvalidOperationException("Terminal EOS is not allowed in LR states");
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            Symbols = s_empty;
        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {
            if (DotLocation == 0)
            {
                Symbols = new IProduction[2] { alternationProduction.Production1, alternationProduction.Production2 };
            }
            else
            {
                Symbols = s_empty;
            }
        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {
            switch (DotLocation)
            {
                case 0:
                    Symbols = new IProduction[1] { concatenationProduction.ProductionLeft };
                    break;
                case 1:
                    Symbols = new IProduction[1] { concatenationProduction.ProductionRight };
                    break;
                default:
                    Symbols = s_empty;
                    break;
            }
        }
    }

    internal class ItemStringVisitor : IProductionVisitor
    {
        public int DotLocation { get; set; }

        private string m_itemString;

        public override string ToString()
        {
            return m_itemString;
        }

        public void VisitTerminal(Terminal terminal)
        {
            throw new NotSupportedException("Terminals do not have item strings");
        }

        public void VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            if (DotLocation == 0)
            {
                m_itemString = mappingProduction.DebugName + " ::=." + mappingProduction.SourceProduction.DebugName;
            }
            else
            {
                m_itemString = mappingProduction.DebugName + " ::= " + mappingProduction.SourceProduction.DebugName + '.';
            }
        }

        public void VisitEndOfStream(EndOfStream endOfStream)
        {
            throw new NotSupportedException("Terminal EOS does not have an item string");
        }

        public void VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            m_itemString = emptyProduction.ToString() + '.';
        }

        public void VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {
            if (DotLocation == 0)
            {
                m_itemString = String.Format("{0} ::=.({1}\\|{2})", alternationProduction.DebugName,
                        alternationProduction.Production1.DebugName,
                        alternationProduction.Production2.DebugName);

            }
            else
            {
                m_itemString = String.Format("{0} ::= ({1}\\|{2}).", alternationProduction.DebugName,
                        alternationProduction.Production1.DebugName,
                        alternationProduction.Production2.DebugName);
            }
        }

        public void VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {
            switch (DotLocation)
            {
                case 0:
                    m_itemString = String.Format("{0} ::=.{1} {2}", concatenationProduction.DebugName,
                        concatenationProduction.ProductionLeft.DebugName,
                        concatenationProduction.ProductionRight.DebugName);
                    break;
                case 1:
                    m_itemString = String.Format("{0} ::= {1}.{2}", concatenationProduction.DebugName,
                        concatenationProduction.ProductionLeft.DebugName,
                        concatenationProduction.ProductionRight.DebugName);
                    break;
                default:
                    m_itemString = concatenationProduction.ToString() + '.';
                    break;
            }
        }
    }
}
