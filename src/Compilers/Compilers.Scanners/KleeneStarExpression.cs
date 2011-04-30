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

        internal override HashSet<char>[] GetCompactableCharSet()
        {
            return InnerExpression.GetCompactableCharSet();
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            return InnerExpression.GetUncompactableCharSet();
        }
    }
}
