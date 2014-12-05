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
    public class ClassDecl : AstNode
    {
        public ClassDecl(LexemeValue name, LexemeValue baseClassName, IList<FieldDecl> fields, IList<MethodDecl> methods)
        {
            BaseClass = new TypeRef(baseClassName);
            Name = name;
            Fields = new ReadOnlyCollection<FieldDecl>(fields);
            Methods = new ReadOnlyCollection<MethodDecl>(methods);
        }

        public TypeRef BaseClass { get; private set; }
        public LexemeValue Name { get; private set; }
        public ReadOnlyCollection<FieldDecl> Fields { get; private set; }
        public ReadOnlyCollection<MethodDecl> Methods { get; private set; }

        public TypeBase Type { get; set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitClassDecl(this);
        }
    }
}
