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
using System.Collections.ObjectModel;

namespace VBF.MiniSharp
{
    public class VariableCollection<T> : KeyedCollection<string, T> where T : VariableInfo
    {
        public int m_Levels;
        private Stack<HashSet<string>> m_levelStack;

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
