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
        internal const int c_SE_TypeNameMissing = 302;
        private const int c_SE_StaticBaseType = 303;
        private const int c_SE_CyclicBaseType = 304;
        private const int c_SE_FieldDuplicates = 310;
        private const int c_SE_MethodDuplicates = 311;
        private const int c_SE_ParameterDuplicates = 312;

        public void DefineErrors()
        {
            m_errorManager.DefineError(c_SE_TypeNameMissing, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' could not be found.");

            m_errorManager.DefineError(c_SE_StaticBaseType, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' is a static class and it can't be used as a base class.");

            m_errorManager.DefineError(c_SE_CyclicBaseType, 0, CompilationStage.SemanticAnalysis,
                "The type '{0}' cannot be use as the base class because it is the same or one of the parent type of '{1}'.");

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

        private TypeBase ResolveTypeRef(TypeRef typeRef)
        {
            TypeBase resolvedType = PrimaryType.Unknown;
            var name = typeRef.TypeName;

            if (!m_types.Contains(name.Content))
            {
                m_errorManager.AddError(c_SE_TypeNameMissing, name.Span, name.Content);
            }
            else
            {
                typeRef.Type = m_types[name.Content];
                resolvedType = typeRef.Type;
            }

            return resolvedType;
        }

        private TypeBase ResolveTypeNode(Ast.Type typeNode)
        {
            Visit(typeNode);
            return typeNode.ResolvedType;
        }

        public override AstNode VisitProgram(Program ast)
        {
            Visit(ast.MainClass);

            foreach (var cd in ast.Classes)
            {
                Visit(cd);
            }

            foreach (var cd in ast.Classes)
            {
                //check cyclic inheritance

                //detect cyclic
                var currentBase = cd.BaseClass.Type;

                while (currentBase != null)
                {
                    if (currentBase == cd.Type)
                    {
                        m_errorManager.AddError(c_SE_CyclicBaseType, cd.BaseClass.TypeName.Span, cd.BaseClass.TypeName.Content, cd.Name.Content);
                        break;
                    }

                    currentBase = (currentBase as CodeClassType).BaseType;
                }
            }

            return ast;
        }

        public override AstNode VisitMainClass(MainClass ast)
        {
            var mainMethod = new Method() { Name = "Main", IsStatic = true };
            mainMethod.ReturnType = PrimaryType.Void;

            var codeType = ast.Type as CodeClassType;

            codeType.StaticMethods.Add(mainMethod);

            return ast;
        }

        public override AstNode VisitClassDecl(ClassDecl ast)
        {
            if (ast.BaseClass.TypeName != null)
            {
                //resolve base class
                var baseTypeName = ast.BaseClass.TypeName.Content;

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
                    (ast.Type as CodeClassType).BaseType = type;
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
            if (declType.Fields.Contains(fieldName.Content))
            {
                m_errorManager.AddError(c_SE_FieldDuplicates, fieldName.Span, declType.Name, fieldName.Content);
            }

            ast.FieldInfo.Name = fieldName.Content;

            var typeNode = ast.Type;
            //check type
            TypeBase resolvedType = ResolveTypeNode(typeNode);

            ast.FieldInfo.Type = resolvedType;
            declType.Fields.Add(ast.FieldInfo);

            return ast;
        }

        public override AstNode VisitMethodDecl(MethodDecl ast)
        {
            var method = ast.MethodInfo;

            method.Name = ast.Name.Content;
            method.IsStatic = false;

            //step 1, resolve return type
            var returnTypeNode = ast.ReturnType;
            var returnType = ResolveTypeNode(returnTypeNode);

            method.ReturnType = returnType;

            //step 2, resolve parameter types
            bool allValid = true;
            HashSet<string> paramNames = new HashSet<string>();

            int paramIndex = 1; //0 is "this"
            foreach (var parameter in ast.Parameters)
            {
                var paramTypeNode = parameter.Type;
                var paramType = ResolveTypeNode(paramTypeNode);

                if (paramType == null)
                {
                    allValid = false;
                }

                var paramInfo = new Parameter() { Name = parameter.ParameterName.Content, Type = paramType, Index = paramIndex };

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

                paramIndex++;
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

            declType.Methods.Add(method);
            return ast;
        }

        public override AstNode VisitIdentifierType(IdentifierType ast)
        {
            ast.ResolvedType = ResolveTypeRef(ast.Type);
            return ast;
        }

        public override AstNode VisitIntArrayType(IntArrayType ast)
        {
            ast.ResolvedType = ArrayType.IntArray;
            return ast;
        }

        public override AstNode VisitIntegerType(IntegerType ast)
        {
            ast.ResolvedType = PrimaryType.Int;
            return ast;
        }

        public override AstNode VisitBooleanType(BooleanType ast)
        {
            ast.ResolvedType = PrimaryType.Boolean;
            return ast;
        }
    }
}
