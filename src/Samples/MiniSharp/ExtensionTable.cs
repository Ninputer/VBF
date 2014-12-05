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
using System.Runtime.CompilerServices;

namespace VBF.MiniSharp
{
    public class ExtensionTable<T>
    {
        private Dictionary<object, T> m_extensionDict;

        public ExtensionTable()
        {
            m_extensionDict = new Dictionary<object, T>(new ReferenceComparer());
        }

        public void Set(object obj, T value)
        {
            m_extensionDict[obj] = value;
        }

        public void Remove(object obj)
        {
            m_extensionDict.Remove(obj);
        }

        public T Get(object obj)
        {
            if (m_extensionDict.ContainsKey(obj))
            {
                return m_extensionDict[obj];
            }
            else
            {
                return default(T);
            }

        }

        private class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }

        }
    }
}
