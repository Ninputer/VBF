using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace VBF.MiniSharp
{
    public class ExtensionTable<T>
    {
        private class ReferenceComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                return Object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }

        }

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
    }
}
