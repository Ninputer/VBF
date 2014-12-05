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
    static class CodeContract
    {
        public static void RequiresArgumentNotNull(object argValue, string argName)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName);
            }
        }

        public static void RequiresArgumentNotNull(object argValue, string argName, string message)
        {
            if (argValue == null)
            {
                throw new ArgumentNullException(argName, message);
            }
        }

        public static void RequiresArgumentInRange(bool isInRange, string argName, string message)
        {
            if (!isInRange)
            {
                throw new ArgumentOutOfRangeException(argName, message);
            }
        }

        public static void Requires(bool condition, string argName)
        {
            if (!condition)
            {
                throw new ArgumentException(argName);
            }
        }

        public static void Requires(bool condition, string argName, string message)
        {
            if (!condition)
            {
                throw new ArgumentException(argName, message);
            }
        }
    }
}
