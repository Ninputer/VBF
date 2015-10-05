using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Parsers
{
    class DefaultValueOfProductionVisitor : IProductionVisitor<bool, object>
    {
        private static DefaultValueOfProductionVisitor s_Instance = 
            new DefaultValueOfProductionVisitor();

        public static DefaultValueOfProductionVisitor Instance
        {
            get
            {
                return s_Instance;
            }
        }

        public object VisitAlternation<T>(AlternationProduction<T> alternationProduction, bool argument)
        {
            return default(T);
        }

        public object VisitConcatenation<T1, T2, TR>(ConcatenationProduction<T1, T2, TR> concatenationProduction, bool argument)
        {
            return default(TR);
        }

        public object VisitEmpty<T>(EmptyProduction<T> emptyProduction, bool argument)
        {
            return default(T);
        }

        public object VisitEndOfStream(EndOfStream endOfStream, bool argument)
        {
            return null;
        }

        public object VisitMapping<TSource, TReturn>(MappingProduction<TSource, TReturn> mappingProduction, bool argument)
        {
            return default(TReturn);
        }

        public object VisitTerminal(Terminal terminal, bool argument)
        {
            return null;
        }
    }
}
