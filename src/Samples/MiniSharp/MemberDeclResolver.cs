using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.MiniSharp.Ast;
using VBF.Compilers;
using System.Diagnostics;

namespace VBF.MiniSharp
{
    public class MemberDeclResolver : AstVisitor
    {
        private TypeCollection m_types;

        private CompilationErrorManager m_errorManager;
        private const int c_SE_TypeNameMissing = 302;
        private const int c_SE_StaticBaseType = 303;
        private const int c_SE_FieldDuplicates = 310;
        private const int c_SE_MethodDuplicates = 311;
        private const int c_SE_ParameterDuplicates = 312;

        public void DefineErrors()
        {
            m_errorManager.DefineError(c_SE_TypeNameMissing, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' could not be found.");

            m_errorManager.DefineError(c_SE_StaticBaseType, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' is a static class and it can't be used as a base class.");

            m_errorManager.DefineError(c_SE_FieldDuplicates, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' has already defined a field named '{1}'.");

            m_errorManager.DefineError(c_SE_MethodDuplicates, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' has already defined a method named '{1}' with same parameter types.");

            m_errorManager.DefineError(c_SE_ParameterDuplicates, 0, CompilationStage.SemanticAnalysis,
                "The method '{0}' has already defined a parameter named '{1}'.");
        }

        public MemberDeclResolver(CompilationErrorManager errorManager, TypeCollection types)
        {
            m_errorManager = errorManager;
            m_types = types;
        }

        private TypeBase ResolveTypeNode(Ast.Type typeNode)
        {
            var idType = typeNode as IdentifierType;
            var intType = typeNode as IntegerType;
            var boolType = typeNode as BooleanType;
            var intArrayType = typeNode as IntArrayType;

            TypeBase resolvedType = null;

            if (idType != null)
            {
                var name = idType.Type.TypeName;

                if (!m_types.Contains(name.Value))
                {
                    m_errorManager.AddError(c_SE_TypeNameMissing, name.Span, name.Value);
                }
                else
                {
                    idType.Type.Type = m_types[name.Value];
                    resolvedType = idType.Type.Type;
                }
            }
            else if (intType != null)
            {
                resolvedType = PrimaryType.Int;
            }
            else if (boolType != null)
            {
                resolvedType = PrimaryType.Boolean;
            }
            else if (intArrayType != null)
            {
                resolvedType = ArrayType.IntArray;
            }
            return resolvedType;
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
            var mainMethod = new Method() { Name = "Main", IsStatic = true };
            mainMethod.ReturnType = CodeClassType.Void;

            var codeType = ast.Type as CodeClassType;

            codeType.StaticMethods.Add(mainMethod);

            return ast;
        }

        public override AstNode VisitClassDecl(ClassDecl ast)
        {
            if (ast.BaseClass.TypeName != null)
            {
                //resolve base class
                var baseTypeName = ast.BaseClass.TypeName.Value;

                if (!m_types.Contains(baseTypeName))
                {
                    m_errorManager.AddError(c_SE_TypeNameMissing, ast.BaseClass.TypeName.Span, baseTypeName);

                    //leave ast.BaseClass.Type empty
                }
                else
                {
                    var type = m_types[baseTypeName] as CodeClassType;

                    Debug.Assert(type != null);

                    if (type.IsStatic)
                    {
                        m_errorManager.AddError(c_SE_StaticBaseType, ast.BaseClass.TypeName.Span, baseTypeName);
                    }

                    ast.BaseClass.Type = type;
                }
            }

            //resolve member decl types

            //fields
            foreach (var field in ast.Fields)
            {
                field.FieldInfo = new Field() { DeclaringType = ast.Type };
                Visit(field);
            }

            //methods
            foreach (var method in ast.Methods)
            {
                method.MethodInfo = new Method() { DeclaringType = ast.Type };
                Visit(method);
            }

            return ast;
        }

        public override AstNode VisitFieldDecl(FieldDecl ast)
        {
            var declType = ast.FieldInfo.DeclaringType as CodeClassType;
            var fieldName = ast.FieldName;
            //check name conflict
            if (declType.Fields.Contains(fieldName.Value))
            {
                m_errorManager.AddError(c_SE_FieldDuplicates, fieldName.Span, declType.Name, fieldName.Value);
            }

            ast.FieldInfo.Name = fieldName.Value;

            var typeNode = ast.Type;
            //check type
            TypeBase resolvedType = ResolveTypeNode(typeNode);

            ast.FieldInfo.Type = resolvedType;

            return ast;
        }

        public override AstNode VisitMethodDecl(MethodDecl ast)
        {
            var method = ast.MethodInfo;

            method.Name = ast.Name.Value;
            method.IsStatic = false;

            //step 1, resolve return type
            var returnTypeNode = ast.ReturnType;
            var returnType = ResolveTypeNode(returnTypeNode);

            method.ReturnType = returnType;

            //step 2, resolve parameter types
            bool allValid = true;
            HashSet<string> paramNames = new HashSet<string>();
            foreach (var parameter in ast.Parameters)
            {
                var paramTypeNode = parameter.Type;
                var paramType = ResolveTypeNode(paramTypeNode);

                if (paramType == null)
                {
                    allValid = false;
                }

                var paramInfo = new Parameter() { Name = parameter.ParameterName.Value, Type = paramType };

                if (paramNames.Contains(paramInfo.Name))
                {
                    m_errorManager.AddError(c_SE_ParameterDuplicates, parameter.ParameterName.Span, method.Name, paramInfo.Name);
                    allValid = false;
                }
                else
                {
                    paramNames.Add(paramInfo.Name);
                    method.Parameters.Add(paramInfo);
                }
            }

            //step 3, check overloading

            if (returnType == null || !allValid)
            {
                //resolve type failed
                return ast;
            }

            var declType = method.DeclaringType as CodeClassType;

            var methodsSameName = declType.Methods.Where(m => m.Name == method.Name).ToArray();
            foreach (var overloadMethod in methodsSameName)
            {
                if (overloadMethod.Parameters.Count == method.Parameters.Count)
                {
                    bool allTypeSame = true;
                    for (int i = 0; i < overloadMethod.Parameters.Count; i++)
                    {
                        if (overloadMethod.Parameters[i].Type != method.Parameters[i].Type)
                        {
                            allTypeSame = false;
                            break;
                        }
                    }

                    if (allTypeSame)
                    {
                        m_errorManager.AddError(c_SE_MethodDuplicates, ast.Name.Span, method.DeclaringType.Name, method.Name);
                    }
                }
            }

            return ast;
        }
    }
}
