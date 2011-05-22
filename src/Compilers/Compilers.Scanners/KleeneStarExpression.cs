using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    public sealed class KleeneStarExpression : RegularExpression
    {
        public RegularExpression InnerExpression { get; private set; }

        public KleeneStarExpression(RegularExpression innerExp)
            : base(RegularExpressionType.KleeneStar)
        {
            CodeContract.RequiresArgumentNotNull(innerExp, "innerExp");

            InnerExpression = innerExp;
        }

        public override string ToString()
        {
            return '(' + InnerExpression.ToString() + ")*";
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return InnerExpression.GetCompactableCharSets();
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            return InnerExpression.GetUncompactableCharSet();
        }

        internal override T Accept<T>(RegularExpressionConverter<T> converter)
        {
            return converter.ConvertKleeneStar(this);
        }
    }
}
