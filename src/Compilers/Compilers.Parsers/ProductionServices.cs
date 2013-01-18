using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    
    public class ProductionServices : IProductionVisitor
    {               
        void IProductionVisitor.VisitTerminal(Terminal terminal)
        {
            throw new NotImplementedException();
        }

        void IProductionVisitor.VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction)
        {
            throw new NotImplementedException();
        }

        void IProductionVisitor.VisitEndOfStream(EndOfStream endOfStream)
        {
            throw new NotImplementedException();
        }

        void IProductionVisitor.VisitEmpty<T>(EmptyProduction<T> emptyProduction)
        {
            throw new NotImplementedException();
        }

        void IProductionVisitor.VisitAlternation<T>(AlternationProduction<T> alternationProduction)
        {
            throw new NotImplementedException();
        }

        void IProductionVisitor.VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction)
        {
            throw new NotImplementedException();
        }
    }
}
