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

namespace VBF.Compilers
{
    public class SourceSpan : IEquatable<SourceSpan>
    {
        private readonly SourceLocation m_endLocation;
        private readonly SourceLocation m_startLocation;

        public SourceSpan(SourceLocation startLocation, SourceLocation endLocation)
        {
            m_startLocation = startLocation;
            m_endLocation = endLocation;
        }

        public SourceLocation StartLocation
        {
            get { return m_startLocation; }
        }

        public SourceLocation EndLocation
        {
            get { return m_endLocation; }
        }

        public bool Equals(SourceSpan other)
        {
            if (other == null)
            {
                return false;
            }

            return m_startLocation.Equals(other.m_startLocation) &&
                   m_endLocation.Equals(other.m_endLocation);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SourceSpan);
        }

        public override int GetHashCode()
        {
            return (m_endLocation.GetHashCode() << 16) ^ m_startLocation.GetHashCode();
        }
    }

}
