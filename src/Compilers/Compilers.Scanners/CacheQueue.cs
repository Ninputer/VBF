using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace VBF.Compilers.Scanners
{
    class CacheQueue<T>
    {
        private T[] m_array;
        private int m_head;
        private int m_tail;
        private int m_size;

        public CacheQueue()
        {
            m_array = new T[4];
            m_head = 0;
            m_tail = 0;
            m_size = 0;
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
    }
}
