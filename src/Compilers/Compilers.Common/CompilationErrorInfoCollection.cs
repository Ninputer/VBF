using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.Compilers
{
    public class CompilationErrorInfoCollection : KeyedCollection<int, CompilationErrorInfo>
    {

        protected override int GetKeyForItem(CompilationErrorInfo item)
        {
            return item.Id;
        }
    }
}
