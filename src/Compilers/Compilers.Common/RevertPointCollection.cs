using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.Compilers
{
    class RevertPointCollection : KeyedCollection<int, RevertPoint>
    {
        protected override int GetKeyForItem(RevertPoint item)
        {
            return item.Key;
        }
    }
}
