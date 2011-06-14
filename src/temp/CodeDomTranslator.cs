using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using Microsoft.CSharp;
using System.IO;

namespace VBF.MiniSharp
{
    public class CodeDomTranslator : Ast.IAstVisitor<CodeObject>
    {
        public CodeObject Visit(VBF.MiniSharp.Ast.AstNode ast)
        {
            return ast.Accept(this);
        }

        private CodeExpression Visit(Ast.VariableRef varRef)
        {
            if (varRef.VariableInfo is Parameter)
            {
                return new CodeArgumentReferenceExpression(varRef.VariableInfo.Name);
            }
            else if (varRef.VariableInfo is Field)
            {
                return new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), varRef.VariableInfo.Name);
            }
            else
            {
                return new CodeVariableReferenceExpression(varRef.VariableInfo.Name);
            }

        }

        public CodeObject VisitArrayAssign(Ast.ArrayAssign ast)
        {
            return new CodeAssignStatement(
                new CodeArrayIndexerExpression(Visit(ast.Array), new[] { Visit(ast.Index) as CodeExpression }),
                Visit(ast.Value) as CodeExpression);
        }

        public CodeObject VisitArrayLength(Ast.ArrayLength ast)
        {
            return new CodePropertyReferenceExpression(
                Visit(ast.Array) as CodeExpression,
                "Length");
        }

        public CodeObject VisitArrayLookup(Ast.ArrayLookup ast)
        {
            return new CodeArrayIndexerExpression(Visit(ast.Array) as CodeExpression, new[] { Visit(ast.Index) as CodeExpression });
        }

        public CodeObject VisitAssign(Ast.Assign ast)
        {
            return new CodeAssignStatement(
                Visit(ast.Variable),
                Visit(ast.Value) as CodeExpression);
        }

        public CodeObject VisitBinary(Ast.Binary ast)
        {
            CodeBinaryOperatorType codeOpType;
            switch (ast.Operator)
            {
                case VBF.MiniSharp.Ast.BinaryOperator.Add:
                    codeOpType = CodeBinaryOperatorType.Add;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.Substract:
                    codeOpType = CodeBinaryOperatorType.Subtract;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.Multiply:
                    codeOpType = CodeBinaryOperatorType.Multiply;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.Divide:
                    codeOpType = CodeBinaryOperatorType.Divide;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.Less:
                    codeOpType = CodeBinaryOperatorType.LessThan;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.Greater:
                    codeOpType = CodeBinaryOperatorType.GreaterThan;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.Equal:
                    codeOpType = CodeBinaryOperatorType.ValueEquality;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.LogicalAnd:
                    codeOpType = CodeBinaryOperatorType.BooleanAnd;
                    break;
                case VBF.MiniSharp.Ast.BinaryOperator.LogicalOr:
                    codeOpType = CodeBinaryOperatorType.BooleanOr;
                    break;
                default:
                    throw new NotSupportedException();
            }

            return new CodeBinaryOperatorExpression(Visit(ast.Left) as CodeExpression, codeOpType, Visit(ast.Right) as CodeExpression);
        }

        private CodeStatement[] RequiresStatementCollection(Ast.Statement ast, bool generateBraces)
        {
            Ast.Block block = ast as Ast.Block;

            if (block == null)
            {
                return new[] { Visit(ast) as CodeStatement };
            }
            else
            {
                List<CodeStatement> result = new List<CodeStatement>();

                if (generateBraces) result.Add(new CodeSnippetStatement("{"));
                foreach (var s in block.Statements)
                {
                    result.AddRange(RequiresStatementCollection(s, true));
                }
                if (generateBraces) result.Add(new CodeSnippetStatement("}"));

                return result.ToArray();
            }
        }

        CodeObject Ast.IAstVisitor<CodeObject>.VisitBlock(Ast.Block ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitBooleanLiteral(Ast.BooleanLiteral ast)
        {
            return new CodePrimitiveExpression(ast.Value);
        }

        CodeObject Ast.IAstVisitor<CodeObject>.VisitBooleanType(Ast.BooleanType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitCall(Ast.Call ast)
        {
            return new CodeMethodInvokeExpression(
                Visit(ast.Target) as CodeExpression,
                ast.Method.MethodInfo.Name,
                (from e in ast.Arguments select Visit(e) as CodeExpression).ToArray());
        }

        public CodeObject VisitClassDecl(Ast.ClassDecl ast)
        {
            CodeTypeDeclaration decl = new CodeTypeDeclaration(ast.Type.Name);

            if (ast.BaseClass.Type != null)
            {
                decl.BaseTypes.Add(new CodeTypeReference(ast.BaseClass.Type.Name));
            }

            foreach (var field in ast.Fields)
            {
                decl.Members.Add(Visit(field) as CodeMemberField);
            }

            foreach (var method in ast.Methods)
            {
                decl.Members.Add(Visit(method) as CodeMemberMethod);
            }

            return decl;
        }

        public CodeObject VisitFieldDecl(Ast.FieldDecl ast)
        {
            return new CodeMemberField(
                new CodeTypeReference(ast.FieldInfo.Type.Name),
                ast.FieldInfo.Name) { Attributes = MemberAttributes.Private };
        }

        CodeObject Ast.IAstVisitor<CodeObject>.VisitFormal(Ast.Formal ast)
        {
            throw new NotImplementedException();
        }

        CodeObject Ast.IAstVisitor<CodeObject>.VisitIdentifierType(Ast.IdentifierType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitIfElse(Ast.IfElse ast)
        {
            return new CodeConditionStatement(
                Visit(ast.Condition) as CodeExpression,
                RequiresStatementCollection(ast.TruePart, false),
                RequiresStatementCollection(ast.FalsePart, false));
        }

        CodeObject Ast.IAstVisitor<CodeObject>.VisitIntArrayType(Ast.IntArrayType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitIntegerLiteral(Ast.IntegerLiteral ast)
        {
            return new CodePrimitiveExpression(ast.Value);
        }

        CodeObject Ast.IAstVisitor<CodeObject>.VisitIntegerType(Ast.IntegerType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitMainClass(Ast.MainClass ast)
        {
            CodeTypeDeclaration decl = new CodeTypeDeclaration(ast.Type.Name);

            CodeMemberMethod mainMethod = new CodeMemberMethod();
            mainMethod.Name = "Main";
            mainMethod.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            mainMethod.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference("System.String", 1), ast.ArgName.Value));

            foreach (var s in ast.Statements)
            {
                var scol = RequiresStatementCollection(s, true);

                foreach (var statement in scol)
                {
                    mainMethod.Statements.Add(statement);
                }
            }

            decl.Members.Add(mainMethod);

            return decl;
        }

        public CodeObject VisitMethodDecl(Ast.MethodDecl ast)
        {
            CodeMemberMethod codemethod = new CodeMemberMethod();
            codemethod.Name = ast.MethodInfo.Name;
            codemethod.ReturnType = new CodeTypeReference(ast.MethodInfo.ReturnType.Name);
            codemethod.Attributes = MemberAttributes.Public;

            //parameters
            foreach (var p in ast.MethodInfo.Parameters)
            {
                codemethod.Parameters.Add(new CodeParameterDeclarationExpression(
                    new CodeTypeReference(p.Type.Name),
                    p.Name));
            }

            //statements
            foreach (var s in ast.Statements)
            {
                var scol = RequiresStatementCollection(s, true);

                foreach (var statement in scol)
                {
                    codemethod.Statements.Add(statement);
                }
            }

            //return method
            codemethod.Statements.Add(new CodeMethodReturnStatement(Visit(ast.ReturnExpression) as CodeExpression));

            return codemethod;
        }

        public CodeObject VisitNewArray(Ast.NewArray ast)
        {
            return new CodeArrayCreateExpression(
                new CodeTypeReference("int"),
                Visit(ast.Length) as CodeExpression);
        }

        public CodeObject VisitNewObject(Ast.NewObject ast)
        {
            return new CodeObjectCreateExpression(
                new CodeTypeReference(ast.Type.Type.Name));
        }

        public CodeObject VisitNot(Ast.Not ast)
        {
            CSharpCodeProvider csprovider = new CSharpCodeProvider();
            var innerExp = Visit(ast.Operand);

            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            csprovider.GenerateCodeFromExpression(innerExp as CodeExpression, sw, new System.CodeDom.Compiler.CodeGeneratorOptions());
            ms.Seek(0, SeekOrigin.Begin);
            StreamReader sr = new StreamReader(ms);

            return new CodeSnippetExpression("!" + sr.ReadToEnd());
        }

        public CodeObject VisitProgram(Ast.Program ast)
        {
            CodeCompileUnit compileUnit = new CodeCompileUnit();

            CodeNamespace ns = new CodeNamespace();
            compileUnit.Namespaces.Add(ns);

            ns.Types.Add(Visit(ast.MainClass) as CodeTypeDeclaration);

            foreach (var c in ast.Classes)
            {
                ns.Types.Add(Visit(c) as CodeTypeDeclaration);
            }

            return compileUnit;
        }

        public CodeObject VisitThis(Ast.This ast)
        {
            return new CodeThisReferenceExpression();
        }

        public CodeObject VisitTypeConvert(Ast.TypeConvert ast)
        {
            return new CodeCastExpression(
                new CodeTypeReference(ast.ExpressionType.Name),
                Visit(ast.Source) as CodeExpression);
        }

        public CodeObject VisitVarDecl(Ast.VarDecl ast)
        {
            return new CodeVariableDeclarationStatement(
                new CodeTypeReference(ast.Type.ResolvedType.Name),
                ast.VariableName.Value);
        }

        public CodeObject VisitVariable(Ast.Variable ast)
        {
            return Visit(ast.VariableRef);
        }

        public CodeObject VisitWhile(Ast.While ast)
        {
            return new CodeIterationStatement(null, Visit(ast.Condition) as CodeExpression, null, RequiresStatementCollection(ast.LoopBody, false));
        }

        public CodeObject VisitWriteLine(Ast.WriteLine ast)
        {
            return new CodeExpressionStatement(
                new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(null, "System.Console.WriteLine"),
                    Visit(ast.Value) as CodeExpression));
        }
    }
}
