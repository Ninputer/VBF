using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp
{
    public class VariableCollection: KeyedCollection<string, VariableInfo>
    {
        protected override string GetKeyForItem(VariableInfo item)
        {
            return item.Name;
        }
    }
}
