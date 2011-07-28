using System;

namespace VBF.Compilers.Scanners
{
    public class Token : IEquatable<Token>
    {
        public int Index { get; private set; }
        public string Description { get; private set; }

        public Token(int index, string description)
        {
            Index = index;
            Description = description;
        }

        public bool Equals(Token other)
        {
            if (other == null)
            {
                return false;
            }

            return Index == other.Index;
        }

        public override int GetHashCode()
        {
            return Index;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Token);
        }

        public override string ToString()
        {
            return Description;
        }
    }
}
