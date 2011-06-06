using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp
{
    public class VariableCollection<T> : KeyedCollection<string, T> where T : VariableInfo
    {
        private Stack<HashSet<string>> m_levelStack;
        public int m_Levels;

        public VariableCollection()
        {
            m_Levels = 0;
            m_levelStack = new Stack<HashSet<string>>();
        }

        protected override string GetKeyForItem(T item)
        {
            return item.Name;
        }

        public void PushLevel()
        {
            m_levelStack.Push(new HashSet<string>());
            m_Levels++;
        }

        public void PopLevel()
        {
            if (m_Levels == 0)
            {
                throw new InvalidOperationException();
            }

            var keysInLevel = m_levelStack.Pop();
            m_Levels--;

            foreach (var key in keysInLevel)
            {
                Remove(key);
            }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (m_Levels > 0)
            {
                var keysInLevel = m_levelStack.Peek();
                keysInLevel.Add(GetKeyForItem(item));
            }


        }
    }
}
