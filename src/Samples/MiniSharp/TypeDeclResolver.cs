using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.MiniSharp.Ast;
using System.Diagnostics;
using VBF.Compilers;

namespace VBF.MiniSharp
{
    public class TypeDeclResolver : AstVisitor
    {
        private TypeCollection m_types;
        private CompilationErrorManager m_errorManager;

        private const int c_SE_TypeNameDuplicates = 301;

        private CompilationErrorList m_errorList;

        public CompilationErrorList ErrorList
        {
            get { return m_errorList; }
            set { m_errorList = value; }
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

        public TypeDeclResolver(CompilationErrorManager errorManager)
        {
            m_errorManager = errorManager;
            m_types = new TypeCollection();
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

        public TypeCollection Types
        {
            get { return m_types; }
        }

    }
}
