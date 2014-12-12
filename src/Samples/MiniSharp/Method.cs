// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.ObjectModel;
using System.Text;

namespace VBF.MiniSharp
{
    public class Method
    {
        public Method()
        {
            Parameters = new Collection<Parameter>();
        }

        public TypeBase DeclaringType { get; set; }
        public string Name { get; set; }
        public TypeBase ReturnType { get; set; }
        public Collection<Parameter> Parameters { get; private set; }
        public bool IsStatic { get; set; }

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
