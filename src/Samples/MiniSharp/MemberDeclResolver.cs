using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.MiniSharp.Ast;
using VBF.Compilers;

namespace VBF.MiniSharp
{
    public class MemberDeclResolver : AstVisitor
    {
        private CompilationErrorManager m_errorManager;
        private const int c_SE_TypeNameMissing = 302;
        private const int c_SE_FieldDuplicates = 310;
        private const int c_SE_MethodDuplicates = 311;
        
        public void DefineErrors()
        {
            m_errorManager.DefineError(c_SE_TypeNameMissing, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' could not be found.");

            m_errorManager.DefineError(c_SE_FieldDuplicates, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' has already defined a field named '{1}'.");

            m_errorManager.DefineError(c_SE_MethodDuplicates, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' has already define a method named '{1}' with same parameter types.");
        }

        public MemberDeclResolver(CompilationErrorManager errorManager)
        {
            m_errorManager = errorManager;
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


    }
}
