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

using System;

namespace VBF.Compilers.Parsers.Generator
{
    public struct LR0Item : IEquatable<LR0Item>
    {
        public LR0Item(int productionIndex, int dotLocation) : this()
        {
            ProductionIndex = productionIndex;
            DotLocation = dotLocation;
        }

        public int ProductionIndex { get; set; }
        public int DotLocation { get; set; }

        public bool Equals(LR0Item other)
        {
            return ProductionIndex == other.ProductionIndex && DotLocation == other.DotLocation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals((LR0Item)obj);
        }

        public override int GetHashCode()
        {
            return (DotLocation << 16) ^ ProductionIndex;
        }
    }
}
