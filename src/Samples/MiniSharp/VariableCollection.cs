using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp
{
    public class VariableCollection<T>: KeyedCollection<string, T> where T: VariableInfo
    {
        protected override string GetKeyForItem(T item)
        {
            return item.Name;
        }
    }
}
