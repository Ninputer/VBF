using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp
{
    public class CodeClassType : TypeBase
    {
        public bool IsStatic { get; set; }
        public Collection<Method> Methods { get; private set; }
        public Collection<Method> StaticMethods { get; private set; }
        public VariableCollection<Field> Fields { get; private set; }

        public CodeClassType()
        {
            Methods = new Collection<Method>();
            StaticMethods = new Collection<Method>();
            Fields = new VariableCollection<Field>();
        }

        public static readonly CodeClassType Void = new CodeClassType() { Name = "Void" };
    }
}
