using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.DataStructures
{
    public enum ExtremeType
    {
        Minimum,
        Maximum
    }

    public class PriorityQueue<T>
    {
        private IComparer<T> m_comparer;

        //return true if left is less/greater than right based on extreme type
        //less in Minimum mode, greater in maximum mode
        private Func<T, T, bool> inRightOrder;
        private List<T> m_binaryHeap;

        private int m_size;

        private bool Less(T v0, T v1)
        {
            return m_comparer.Compare(v0, v1) < 0;
        }

        private bool Greater(T v0, T v1)
        {
            return m_comparer.Compare(v0, v1) > 0;
        }

        public PriorityQueue(IEnumerable<T> values, ExtremeType type, IComparer<T> comparer)
        {
            CodeContract.RequiresArgumentNotNull(comparer, "comparer");

            m_comparer = comparer;

            if (type == ExtremeType.Minimum)
            {
                inRightOrder = Less;
            }
            else if (type == ExtremeType.Maximum)
            {
                inRightOrder = Greater;
            }
            else
            {
                throw new ArgumentOutOfRangeException("type", "ExtremeType can only be Minmun or Maximum");
            }

            //initialize prioirty queue heap
            if (values != null)
            {
                m_binaryHeap = new List<T>(values);
            }
            else
            {
                m_binaryHeap = new List<T>();
            }

            InitializeHeap();
        }

        private void InitializeHeap()
        {
            //step 1: swap heap[0] to heap[count], make heap[0] a placeholder
            int count = m_binaryHeap.Count;

            if (count > 0)
            {
                m_binaryHeap.Add(m_binaryHeap[0]);
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
            int child;
            int hole = index;
            T temp = m_binaryHeap[hole];

            while (hole * 2 <= m_size)
            {
                child = hole * 2;

                if (child != m_size &&
                    inRightOrder(m_binaryHeap[child + 1], m_binaryHeap[child]))
                {
                    child++;
                }

                if (inRightOrder(m_binaryHeap[child], temp))
                {
                    m_binaryHeap[hole] = m_binaryHeap[child];

                    //TODO
                }
                else
                {
                    break;
                }

                hole = child;
            }

            m_binaryHeap[hole] = temp;

            //TODO
        }

        private void PercolateUp(int index)
        {
            T temp = m_binaryHeap[index];
            int hole = index;

            while (hole > 1 && inRightOrder(temp, m_binaryHeap[hole / 2]))
            {
                m_binaryHeap[hole] = m_binaryHeap[hole / 2];
                //TODO
                hole /= 2;
            }

            m_binaryHeap[hole] = temp;
            //TEMP
        }

        public PriorityQueue(IEnumerable<T> values, ExtremeType type) : this(values, type, Comparer<T>.Default) { }

        public PriorityQueue(ExtremeType type) : this(null, type, Comparer<T>.Default) { }

        public PriorityQueue() : this(null, ExtremeType.Minimum, Comparer<T>.Default) { }

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
            //TODO
            m_size--;

            PercolateDown(1);

            return extreme;
        }

        public void Insert(T value)
        {
            if (m_size == m_binaryHeap.Count)
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

            //TODO
            PercolateUp(m_size);
        }

        public bool IsEmpty
        {
            get
            {
                return m_size == 0;
            }
        }
    }
}
