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
using System.Collections.ObjectModel;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class Call : Expression
    {
        public Call(Expression target, LexemeValue methodName, IList<Expression> argList)
        {
            Target = target;
            Method = new MethodRef(methodName);
            Arguments = new ReadOnlyCollection<Expression>(argList);
        }

        public Expression Target { get; private set; }
        public MethodRef Method { get; private set; }
        public ReadOnlyCollection<Expression> Arguments { get; private set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitCall(this);
        }
    }
}
