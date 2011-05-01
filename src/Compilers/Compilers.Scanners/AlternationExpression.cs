using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    /// Represents an alternation of two regular expressions 
    /// </summary>
    public sealed class AlternationExpression : RegularExpression
    {
        public RegularExpression Expression1 { get; private set; }
        public RegularExpression Expression2 { get; private set; }

        public AlternationExpression(RegularExpression expression1, RegularExpression expression2)
            : base(RegularExpressionType.Alternation)
        {
            CodeContract.RequiresArgumentNotNull(expression1, "expression1");
            CodeContract.RequiresArgumentNotNull(expression2, "expression2");

            Expression1 = expression1;
            Expression2 = expression2;
        }

        public override string ToString()
        {
            return '(' + Expression1.ToString() + '|' + Expression2.ToString() +')';
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return Expression1.GetCompactableCharSets().Union(Expression2.GetCompactableCharSets()).ToArray();
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            var result = Expression1.GetUncompactableCharSet();
            result.UnionWith(Expression2.GetUncompactableCharSet());

            return result;
        }
    }
}
