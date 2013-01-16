using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers
{
    sealed class RepeatParserListNode<T> : IEnumerable<T>
    {
        public T Value { get; private set; }
        public RepeatParserListNode<T> Next { get; private set; }

        public RepeatParserListNode(T value, RepeatParserListNode<T> next)
        {
            Value = value;
            Next = next;
        }

        public RepeatParserListNode() : this(default(T), null) { }


        public IEnumerator<T> GetEnumerator()
        {
            RepeatParserListNode<T> current = this;

            while (current.Next != null)
            {
                yield return current.Value;

                current = current.Next;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}