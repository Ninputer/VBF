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
using System.Collections.Generic;

namespace VBF.Compilers.Common
{
    public enum ExtremeType
    {
        Minimum,
        Maximum
    }

    public class PriorityQueue<T>
    {
        private List<T> m_binaryHeap;
        private IComparer<T> m_comparer;

        //return true if left is less/greater than right based on extreme type
        //less in Minimum mode, greater in maximum mode
        private Func<T, T, bool> m_inRightOrder;
        private Dictionary<T, int> m_indexDict;

        private int m_size;

        public PriorityQueue(IEnumerable<T> values, ExtremeType type, IComparer<T> comparer)
        {
            CodeContract.RequiresArgumentNotNull(comparer, "comparer");

            m_comparer = comparer;

            switch (type)
            {
                case ExtremeType.Minimum:
                    m_inRightOrder = Less;
                    break;
                case ExtremeType.Maximum:
                    m_inRightOrder = Greater;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type", "ExtremeType can only be Minmun or Maximum");
            }

            m_indexDict = new Dictionary<T, int>();

            //initialize prioirty queue heap
            if (values != null)
            {
                m_binaryHeap = new List<T>(values);

                for (int i = 0; i < m_binaryHeap.Count; i++)
                {
                    m_indexDict.Add(m_binaryHeap[i], i);
                }
            }
            else
            {
                m_binaryHeap = new List<T>();
            }


            InitializeHeap();
        }

        public PriorityQueue(IEnumerable<T> values, ExtremeType type) : this(values, type, Comparer<T>.Default) { }

        public PriorityQueue(ExtremeType type) : this(null, type, Comparer<T>.Default) { }

        public PriorityQueue() : this(null, ExtremeType.Minimum, Comparer<T>.Default) { }

        public bool IsEmpty
        {
            get
            {
                return m_size == 0;
            }
        }

        private bool Less(T v0, T v1)
        {
            return m_comparer.Compare(v0, v1) < 0;
        }

        private bool Greater(T v0, T v1)
        {
            return m_comparer.Compare(v0, v1) > 0;
        }

        private void InitializeHeap()
        {
            //step 1: swap heap[0] to heap[count], make heap[0] a placeholder
            int count = m_binaryHeap.Count;

            if (count > 0)
            {
                m_binaryHeap.Add(m_binaryHeap[0]);
                m_indexDict[m_binaryHeap[0]] = m_binaryHeap.Count - 1;
                m_binaryHeap[0] = default(T);
            }
            else
            {
                m_binaryHeap.Add(default(T));
            }

            m_size = count;

            for (int i = m_size / 2; i > 0; i--)
            {
                PercolateDown(i);
            }
        }

        private void PercolateDown(int index)
        {
            int hole = index;
            T temp = m_binaryHeap[hole];

            while (hole * 2 <= m_size)
            {
                int child = hole * 2;

                if (child != m_size &&
                    m_inRightOrder(m_binaryHeap[child + 1], m_binaryHeap[child]))
                {
                    child++;
                }

                if (m_inRightOrder(m_binaryHeap[child], temp))
                {
                    m_binaryHeap[hole] = m_binaryHeap[child];
                    m_indexDict[m_binaryHeap[hole]] = hole;
                }
                else
                {
                    break;
                }

                hole = child;
            }

            m_binaryHeap[hole] = temp;
            m_indexDict[temp] = hole;
        }

        private void PercolateUp(int index)
        {
            T temp = m_binaryHeap[index];
            int hole = index;

            while (hole > 1 && m_inRightOrder(temp, m_binaryHeap[hole / 2]))
            {
                m_binaryHeap[hole] = m_binaryHeap[hole / 2];
                m_indexDict[m_binaryHeap[hole]] = hole;
                hole /= 2;
            }

            m_binaryHeap[hole] = temp;
            m_indexDict[temp] = hole;
        }

        public T PeekExtreme()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Priority queue is empty");
            }

            return m_binaryHeap[1];
        }

        public T DeleteExtreme()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Priority queue is empty");
            }

            T extreme = m_binaryHeap[1];

            m_binaryHeap[1] = m_binaryHeap[m_size];
            m_indexDict[m_binaryHeap[1]] = 1;
            m_size--;

            PercolateDown(1);

            return extreme;
        }

        public void Insert(T value)
        {
            if (m_size == m_binaryHeap.Count - 1)
            {
                //full, add one
                m_binaryHeap.Add(value);
                m_size += 1;
            }
            else
            {
                m_size += 1;
                m_binaryHeap[m_size] = value;          
            }

            m_indexDict[value] = m_size;
            PercolateUp(m_size);
        }

        public void ModifyValue(T originalValue, T newValue)
        {
            int current;
            if (!m_indexDict.TryGetValue(originalValue, out current))
            {
                throw new ArgumentOutOfRangeException("originalValue", "The originalValue is not in the priority queue");
            }

            m_binaryHeap[current] = newValue;

            if (m_inRightOrder(newValue, originalValue))
            {
                //near extreme, go up
                PercolateUp(current);
            }
            else
            {
                //far from extreme, go down
                PercolateDown(current);
            }
        }
    }
}
