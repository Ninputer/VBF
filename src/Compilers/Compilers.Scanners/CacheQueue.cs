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

using System;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    class CacheQueue<T>
    {
        private T[] m_array;
        private int m_head;
        private int m_size;
        private int m_tail;

        public CacheQueue()
        {
            m_array = new T[4];
            m_head = 0;
            m_tail = 0;
            m_size = 0;
        }

        public int Count
        {
            get
            {
                return m_size;
            }
        }

        public T this[int index]
        {
            get
            {
                Debug.Assert(index >= 0 && index < m_size);
                int indexInArray = (m_head + index) % m_array.Length;

                return m_array[indexInArray];
            }
        }

        private void SetCapacity(int capacity)
        {
            T[] destinationArray = new T[capacity];
            if (m_size > 0)
            {
                if (m_head < m_tail)
                {
                    Array.Copy(m_array, m_head, destinationArray, 0, m_size);
                }
                else
                {
                    Array.Copy(m_array, m_head, destinationArray, 0, m_array.Length - m_head);
                    Array.Copy(m_array, 0, destinationArray, m_array.Length - m_head, m_tail);
                }
            }
            m_array = destinationArray;
            m_head = 0;
            m_tail = (m_size == capacity) ? 0 : m_size;
        }

        public void Enqueue(T item)
        {
            if (m_size == m_array.Length)
            {
                int newCapacity = m_size * 2;

                SetCapacity(newCapacity);
            }

            m_array[m_tail] = item;
            m_tail = (m_tail + 1) % m_array.Length;

            m_size++;
        }

        public T Dequeue()
        {
            Debug.Assert(m_size > 0);

            T item = m_array[m_head];

            //clear reference for GC to collect it
            m_array[m_head] = default(T);

            m_head = (m_head + 1) % m_array.Length;
            m_size--;

            return item;
        }
    }
}
