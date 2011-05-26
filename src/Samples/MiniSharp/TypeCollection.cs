using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp
{
    public class TypeCollection : KeyedCollection<string, TypeBase>
    {
        protected override string GetKeyForItem(TypeBase item)
        {
            return item.Name;
        }
    }
}
