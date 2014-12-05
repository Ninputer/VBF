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

namespace VBF.MiniSharp
{
    public class ArrayType : TypeBase
    {
        public static readonly ArrayType IntArray = new ArrayType() { Name = "int[]", ElementType = PrimaryType.Int };
        public static readonly ArrayType StrArray = new ArrayType() { Name = "string[]", ElementType = PrimaryType.String };
        public TypeBase ElementType { get; set; }

        public override bool IsAssignableFrom(TypeBase type)
        {
            CodeClassType elementClassType = ElementType as CodeClassType;
            ArrayType arrayType = type as ArrayType;

            if (elementClassType != null && arrayType != null)
            {
                return elementClassType.IsAssignableFrom(arrayType.ElementType);
            }

            return false;
        }
    }
}
