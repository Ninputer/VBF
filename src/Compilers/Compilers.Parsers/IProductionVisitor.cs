using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    public interface IProductionVisitor
    {
        void VisitTerminal(Terminal terminal);

        void VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction);

        void VisitEndOfStream(EndOfStream endOfStream);

        void VisitEmpty<T>(EmptyProduction<T> emptyProduction);

        void VisitAlternation<T>(AlternationProduction<T> alternationProduction);

        void VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction);
    }
}
