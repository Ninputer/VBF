using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace VBF.Compilers.Parsers.Combinators
{
    class ConvertHelper<TFrom, TTo>
    {
        private static Func<TFrom, TTo> s_castFunc;

        static ConvertHelper()
        {
            var source = Expression.Parameter(typeof(TFrom), "source");
            s_castFunc = Expression.Lambda<Func<TFrom, TTo>>(Expression.Convert(source, typeof(TTo)), source).Compile();
        }

        public static TTo Convert(TFrom source)
        {
            return s_castFunc(source);
        }
    }
}
