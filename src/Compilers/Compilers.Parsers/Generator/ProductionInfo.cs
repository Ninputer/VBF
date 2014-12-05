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

using System.Collections.Generic;

namespace VBF.Compilers.Parsers.Generator
{
    public class ProductionInfo
    {
        public ProductionInfo()
        {
            First = new HashSet<IProduction>();
            Follow = new HashSet<IProduction>();
            IsNullable = false;
        }

        public ISet<IProduction> First { get; private set; }
        public ISet<IProduction> Follow { get; private set; }
        public bool IsNullable { get; internal set; }

        internal int Index { get; set; }
        internal int SymbolCount { get; set; }
        internal int NonTerminalIndex { get; set; }
    }
    
}
