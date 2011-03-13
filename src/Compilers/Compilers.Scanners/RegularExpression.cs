using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// The base class of regular expressions. Provides methods to build a regular expression
    /// </summary>
    public abstract class RegularExpression
    {
        public RegularExpressionType ExpressionType { get; private set; }

        protected RegularExpression(RegularExpressionType expType)
        {
            ExpressionType = expType;
        }

        //basic operations

        public static RegularExpression Symbol(char c)
        {
            return new SymbolExpression(c);
        }

        public RegularExpression Many()
        {
            if (ExpressionType == RegularExpressionType.KleeneStar)
            {
                return this;
            }

            return new KleeneStarExpression(this);
        }

        public RegularExpression Sequence(RegularExpression follow)
        {
            return new ConcatenationExpression(this, follow);
        }

        public RegularExpression Union(RegularExpression other)
        {
            if (this.Equals(other))
            {
                return this;
            }

            return new AlternationExpression(this, other);
        }

        public static RegularExpression Literal(string literal)
        {
            return new StringLiteralExpression(literal);
        }

        public static RegularExpression CharSet(IEnumerable<char> charSet)
        {
            return new AlternationCharSetExpression(charSet);
        }

        public static RegularExpression Empty()
        {
            return EmptyExpression.Instance;
        }

        //extended operations

        public RegularExpression Many1()
        {
            return this.Sequence(this.Many());
        }

        public RegularExpression Optional()
        {
            return this.Union(Empty());
        }

        public static RegularExpression Range(char min, char max)
        {
            CodeContract.Requires(min <= max, "max", "The lower bound must be less than or equal to upper bound");

            List<char> rangeCharSet = new List<char>();
            for (char c = min; c <= max; c++)
            {
                rangeCharSet.Add(c);
            }

            return new AlternationCharSetExpression(rangeCharSet);
        }

        public RegularExpression Repeat(int number)
        {
            if (number <= 0)
            {
                return Empty();
            }

            RegularExpression result = this;

            for (int i = 1; i < number; i++)
            {
                result = result.Sequence(this);
            }

            return result;
        }

        //operator overloading

        public static RegularExpression operator|(RegularExpression left, RegularExpression right)
        {
            return new AlternationExpression(left, right);
        }

        [SpecialName]
        public static RegularExpression op_RightShift(RegularExpression left, RegularExpression right)
        {
            return new ConcatenationExpression(left, right);
        }
    }
}
