using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp
{
    public class Method
    {
        public TypeBase DeclaringType { get; set; }
        public string Name { get; set; }
        public TypeBase ReturnType { get; set; }
        public Collection<Parameter> Parameters { get; private set; }
        public bool IsStatic { get; set; }

        public Method()
        {
            Parameters = new Collection<Parameter>();
        }
    }
}
