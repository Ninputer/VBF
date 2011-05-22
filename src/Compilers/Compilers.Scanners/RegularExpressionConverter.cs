using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// Used to convert a regular expression to a value. One must inherit this class to implement custom logic.
    /// </summary>
    /// <typeparam name="T">The target type that the converter converts a regular expression to</typeparam>
    public abstract class RegularExpressionConverter<T>
    {
        protected RegularExpressionConverter() { }

        public T Convert(RegularExpression expression)
        {
            if (expression == null)
            {
                return default(T);
            }

            return expression.Accept(this);
        }

        public abstract T ConvertAlternation(AlternationExpression exp);

        public abstract T ConvertSymbol(SymbolExpression exp);

        public abstract T ConvertEmpty(EmptyExpression exp);

        public abstract T ConvertConcatenation(ConcatenationExpression exp);

        public abstract T ConvertAlternationCharSet(AlternationCharSetExpression exp);

        public abstract T ConvertStringLiteral(StringLiteralExpression exp);

        public abstract T ConvertKleeneStar(KleeneStarExpression exp);
    }
}
