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

        public string GetSignatureString()
        {
            StringBuilder signatureBuilder = new StringBuilder();
            signatureBuilder.Append(DeclaringType.Name)
            .Append('.')
            .Append(Name)
            .Append('(');

            for (int i = 0; i < Parameters.Count - 1; i++)
            {
                signatureBuilder.Append(Parameters[i].Type.Name)
                    .Append(',')
                    .Append(' ');
            }

            if (Parameters.Count > 0)
            {
                signatureBuilder.Append(Parameters[Parameters.Count - 1].Type.Name);
            }

            signatureBuilder.Append(')');

            return signatureBuilder.ToString();
        }
    }
}
