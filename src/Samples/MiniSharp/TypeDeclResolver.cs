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

using System.Diagnostics;
using VBF.Compilers;
using VBF.MiniSharp.Ast;

namespace VBF.MiniSharp
{
    public class TypeDeclResolver : AstVisitor
    {
        private const int c_SE_TypeNameDuplicates = 301;

        private CompilationErrorList m_errorList;
        private CompilationErrorManager m_errorManager;
        private TypeCollection m_types;

        public TypeDeclResolver(CompilationErrorManager errorManager)
        {
            m_errorManager = errorManager;
            m_types = new TypeCollection();
        }

        public CompilationErrorList ErrorList
        {
            get { return m_errorList; }
            set { m_errorList = value; }
        }

        public TypeCollection Types
        {
            get { return m_types; }
        }

        private void AddError(int errorId, SourceSpan errorPosition, params object[] args)
        {
            if (m_errorList != null)
            {
                m_errorList.AddError(errorId, errorPosition, args);
            }
        }
        
        public void DefineErrors()
        {
            m_errorManager.DefineError(c_SE_TypeNameDuplicates, 0, CompilationStage.SemanticAnalysis,
                "The program has already defined a type named '{0}'.");
        }

        public override AstNode VisitProgram(Program ast)
        {
            Visit(ast.MainClass);

            foreach (var cd in ast.Classes)
            {
                Visit(cd);
            }

            return ast;
        }

        public override AstNode VisitMainClass(MainClass ast)
        {
            //main class must be the first class.
            Debug.Assert(m_types.Count == 0);
            var name = ast.Name.Content;

            var mainclassType = new CodeClassType() { Name = name, IsStatic = true };

            m_types.Add(mainclassType);
            ast.Type = mainclassType;

            return ast;
        }

        public override AstNode VisitClassDecl(ClassDecl ast)
        {
            var name = ast.Name.Content;

            if (m_types.Contains(name))
            {
                AddError(c_SE_TypeNameDuplicates, ast.Name.Span, name);
                return ast;
            }

            var classType = new CodeClassType() { Name = name };

            m_types.Add(classType);
            ast.Type = classType;

            return ast;
        }
    }
}
