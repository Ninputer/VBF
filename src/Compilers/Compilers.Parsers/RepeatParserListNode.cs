// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;

namespace VBF.Compilers.Parsers
{
    sealed class RepeatParserListNode<T> : IEnumerable<T>
    {
        public RepeatParserListNode(T value, RepeatParserListNode<T> next)
        {
            Value = value;
            Next = next;
        }

        public RepeatParserListNode() : this(default(T), null) { }
        public T Value { get; private set; }
        public RepeatParserListNode<T> Next { get; private set; }


        public IEnumerator<T> GetEnumerator()
        {
            RepeatParserListNode<T> current = this;

            while (current.Next != null)
            {
                yield return current.Value;

                current = current.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}