using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    /// <summary>
    ///  Represents a concatenation of two regular expressions 
    /// </summary>
    public sealed class ConcatenationExpression : RegularExpression
    {
        public RegularExpression Left { get; private set; }
        public RegularExpression Right { get; private set; }

        public ConcatenationExpression(RegularExpression left, RegularExpression right)
            : base(RegularExpressionType.Concatenation)
        {
            CodeContract.RequiresArgumentNotNull(left, "left");
            CodeContract.RequiresArgumentNotNull(right, "right");

            Left = left;
            Right = right;
        }

        public override string ToString()
        {
            return Left.ToString() + Right.ToString();
        }

        internal override Func<HashSet<char>>[] GetCompactableCharSets()
        {
            return Left.GetCompactableCharSets().Union(Right.GetCompactableCharSets()).ToArray();
        }

        internal override HashSet<char> GetUncompactableCharSet()
        {
            var result = Left.GetUncompactableCharSet();
            result.UnionWith(Right.GetUncompactableCharSet());

            return result;
        }
    }
}
