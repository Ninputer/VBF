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
        public CodeClassType BaseType { get; set; }
        public Collection<Method> Methods { get; private set; }
        public Collection<Method> StaticMethods { get; private set; }
        public VariableCollection<Field> Fields { get; private set; }

        public CodeClassType()
        {
            Methods = new Collection<Method>();
            StaticMethods = new Collection<Method>();
            Fields = new VariableCollection<Field>();
        }

        public override bool IsAssignableFrom(TypeBase type)
        {
            CodeClassType otherClassType = type as CodeClassType;

            if (otherClassType == null)
            {
                return false;
            }

            if (otherClassType == this)
            {
                return true;
            }
            else
            {
                return IsAssignableFrom(otherClassType.BaseType);
            }
        }

    }
}
