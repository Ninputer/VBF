using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Parsers.Generator;

namespace VBF.Compilers.Parsers.Generator
{
    internal class ProductionAggregationVisitor : IProductionVisitor<List<IProduction>, List<IProduction>>
    {

        List<IProduction> IProductionVisitor<List<IProduction>, List<IProduction>>.VisitTerminal(Terminal terminal, List<IProduction> Productions)
        {
            if (terminal.Info != null)
            {
                return Productions;
            }

            terminal.Info = new ProductionInfo();
            terminal.Info.First.Add(terminal);

            terminal.Info.Index = Productions.Count;
            terminal.Info.SymbolCount = 1;

            Productions.Add(terminal);

            return Productions;
        }

        List<IProduction> IProductionVisitor<List<IProduction>, List<IProduction>>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, List<IProduction> Productions)
        {
            if (mappingProduction.Info != null)
            {
                return Productions;
            }

            mappingProduction.Info = new ProductionInfo();
            mappingProduction.SourceProduction.Accept(this, Productions);

            mappingProduction.Info.Index = Productions.Count;
            mappingProduction.Info.SymbolCount = 1;
            Productions.Add(mappingProduction);

            return Productions;
        }

        List<IProduction> IProductionVisitor<List<IProduction>, List<IProduction>>.VisitEndOfStream(EndOfStream endOfStream, List<IProduction> Productions)
        {
            if (endOfStream.Info != null)
            {
                return Productions;
            }

            endOfStream.Info = new ProductionInfo();
            endOfStream.Info.First.Add(endOfStream);

            endOfStream.Info.Index = Productions.Count;
            endOfStream.Info.SymbolCount = 1;

            Productions.Add(endOfStream);

            return Productions;
        }

        List<IProduction> IProductionVisitor<List<IProduction>, List<IProduction>>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, List<IProduction> Productions)
        {
            if (emptyProduction.Info != null)
            {
                return Productions;
            }

            emptyProduction.Info = new ProductionInfo();
            emptyProduction.Info.IsNullable = true;

            emptyProduction.Info.Index = Productions.Count;
            emptyProduction.Info.SymbolCount = 0;
            Productions.Add(emptyProduction);

            return Productions;
        }

        List<IProduction> IProductionVisitor<List<IProduction>, List<IProduction>>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, List<IProduction> Productions)
        {
            if (alternationProduction.Info != null)
            {
                return Productions;
            }

            alternationProduction.Info = new ProductionInfo();

            alternationProduction.Production1.Accept(this, Productions);
            alternationProduction.Production2.Accept(this, Productions);

            alternationProduction.Info.Index = Productions.Count;
            alternationProduction.Info.SymbolCount = 1;
            Productions.Add(alternationProduction);

            return Productions;
        }

        List<IProduction> IProductionVisitor<List<IProduction>, List<IProduction>>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, List<IProduction> Productions)
        {
            if (concatenationProduction.Info != null)
            {
                return Productions;
            }

            concatenationProduction.Info = new ProductionInfo();

            concatenationProduction.ProductionLeft.Accept(this, Productions);
            concatenationProduction.ProductionRight.Accept(this, Productions);

            concatenationProduction.Info.Index = Productions.Count;
            concatenationProduction.Info.SymbolCount = 2;
            Productions.Add(concatenationProduction);

            return Productions;
        }       
    }

    internal class FirstFollowVisitor : IProductionVisitor<bool, bool>
    {
        bool IProductionVisitor<bool, bool>.VisitTerminal(Terminal terminal, bool IsChanged)
        {
            return IsChanged;
        }

        bool IProductionVisitor<bool, bool>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, bool IsChanged)
        {
            var source = mappingProduction.SourceProduction;

            IsChanged = mappingProduction.Info.First.UnionCheck(source.Info.First) || IsChanged;
            IsChanged = source.Info.Follow.UnionCheck(mappingProduction.Info.Follow) || IsChanged;

            if (mappingProduction.Info.IsNullable != source.Info.IsNullable)
            {
                mappingProduction.Info.IsNullable = source.Info.IsNullable;
                IsChanged = true;
            }

            return IsChanged;
        }

        bool IProductionVisitor<bool, bool>.VisitEndOfStream(EndOfStream endOfStream, bool IsChanged)
        {
            return IsChanged;
        }

        bool IProductionVisitor<bool, bool>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, bool IsChanged)
        {
            return IsChanged;
        }

        bool IProductionVisitor<bool, bool>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, bool IsChanged)
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

            return IsChanged;
        }

        bool IProductionVisitor<bool, bool>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, bool IsChanged)
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

            return IsChanged;

        }


    }

    internal class DotSymbolVisitor : IProductionVisitor<int, IReadOnlyList<IProduction>>
    {
        private static readonly IProduction[] s_empty = new IProduction[0];

        IReadOnlyList<IProduction> IProductionVisitor<int, IReadOnlyList<IProduction>>.VisitTerminal(Terminal terminal, int DotLocation)
        {
            throw new InvalidOperationException("Terminals are not allowed in LR states");
        }

        IReadOnlyList<IProduction> IProductionVisitor<int, IReadOnlyList<IProduction>>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, int DotLocation)
        {
            if (DotLocation == 0)
            {
                return new IProduction[1] { mappingProduction.SourceProduction };
            }
            else
            {
                return s_empty;
            }
        }

        IReadOnlyList<IProduction> IProductionVisitor<int, IReadOnlyList<IProduction>>.VisitEndOfStream(EndOfStream endOfStream, int DotLocation)
        {
            throw new InvalidOperationException("Terminal EOS is not allowed in LR states");
        }

        IReadOnlyList<IProduction> IProductionVisitor<int, IReadOnlyList<IProduction>>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, int DotLocation)
        {
            return s_empty;
        }

        IReadOnlyList<IProduction> IProductionVisitor<int, IReadOnlyList<IProduction>>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, int DotLocation)
        {
            if (DotLocation == 0)
            {
                return new IProduction[2] { alternationProduction.Production1, alternationProduction.Production2 };
            }
            else
            {
                return s_empty;
            }
        }

        IReadOnlyList<IProduction> IProductionVisitor<int, IReadOnlyList<IProduction>>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, int DotLocation)
        {
            switch (DotLocation)
            {
                case 0:
                    return new IProduction[1] { concatenationProduction.ProductionLeft };
                case 1:
                    return new IProduction[1] { concatenationProduction.ProductionRight };
                default:
                    return s_empty;
            }
        }
    }

    internal class ItemStringVisitor : IProductionVisitor<int, string>
    {

        string IProductionVisitor<int, string>.VisitTerminal(Terminal terminal, int DotLocation)
        {
            throw new NotSupportedException("Terminals do not have item strings");
        }

        string IProductionVisitor<int, string>.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, int DotLocation)
        {
            if (DotLocation == 0)
            {
                return mappingProduction.DebugName + " ::=." + mappingProduction.SourceProduction.DebugName;
            }
            else
            {
                return mappingProduction.DebugName + " ::= " + mappingProduction.SourceProduction.DebugName + '.';
            }
        }

        string IProductionVisitor<int, string>.VisitEndOfStream(EndOfStream endOfStream, int DotLocation)
        {
            throw new NotSupportedException("Terminal EOS does not have an item string");
        }

        string IProductionVisitor<int, string>.VisitEmpty<T>(EmptyProduction<T> emptyProduction, int DotLocation)
        {
            return emptyProduction.ToString() + '.';
        }

        string IProductionVisitor<int, string>.VisitAlternation<T>(AlternationProduction<T> alternationProduction, int DotLocation)
        {
            if (DotLocation == 0)
            {
                return String.Format("{0} ::=.({1}|{2})", alternationProduction.DebugName,
                        alternationProduction.Production1.DebugName,
                        alternationProduction.Production2.DebugName);

            }
            else
            {
                return String.Format("{0} ::= ({1}|{2}).", alternationProduction.DebugName,
                        alternationProduction.Production1.DebugName,
                        alternationProduction.Production2.DebugName);
            }
        }

        string IProductionVisitor<int, string>.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, int DotLocation)
        {
            switch (DotLocation)
            {
                case 0:
                    return String.Format("{0} ::=.{1} {2}", concatenationProduction.DebugName,
                        concatenationProduction.ProductionLeft.DebugName,
                        concatenationProduction.ProductionRight.DebugName);
                case 1:
                    return String.Format("{0} ::= {1}.{2}", concatenationProduction.DebugName,
                        concatenationProduction.ProductionLeft.DebugName,
                        concatenationProduction.ProductionRight.DebugName);
                default:
                    return concatenationProduction.ToString() + '.';
            }
        }
    }
}
