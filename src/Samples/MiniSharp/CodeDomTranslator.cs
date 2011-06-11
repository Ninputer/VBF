using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.CodeDom;

namespace VBF.MiniSharp
{
    public class CodeDomTranslator : Ast.IAstVisitor<CodeObject>
    {
        public CodeObject VisitArrayAssign(Ast.ArrayAssign ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitArrayLength(Ast.ArrayLength ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitArrayLookup(Ast.ArrayLookup ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitAssign(Ast.Assign ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitBinary(Ast.Binary ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitBlock(Ast.Block ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitBooleanLiteral(Ast.BooleanLiteral ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitBooleanType(Ast.BooleanType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitCall(Ast.Call ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitClassDecl(Ast.ClassDecl ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitFieldDecl(Ast.FieldDecl ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitFormal(Ast.Formal ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitIdentifierType(Ast.IdentifierType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitIfElse(Ast.IfElse ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitIntArrayType(Ast.IntArrayType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitIntegerLiteral(Ast.IntegerLiteral ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitIntegerType(Ast.IntegerType ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitMainClass(Ast.MainClass ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitMethodDecl(Ast.MethodDecl ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitNewArray(Ast.NewArray ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitNewObject(Ast.NewObject ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitNot(Ast.Not ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitProgram(Ast.Program ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitThis(Ast.This ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitTypeConvert(Ast.TypeConvert ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitVarDecl(Ast.VarDecl ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitVariable(Ast.Variable ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitWhile(Ast.While ast)
        {
            throw new NotImplementedException();
        }

        public CodeObject VisitWriteLine(Ast.WriteLine ast)
        {
            throw new NotImplementedException();
        }
    }
}
