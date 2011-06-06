using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public abstract class AstVisitor : IAstVisitor<AstNode>
    {
        protected AstVisitor() { }

        public AstNode Visit(AstNode ast)
        {
            return ast.Accept(this);
        }

        public virtual AstNode VisitArrayAssign(ArrayAssign ast)
        {
            return ast;
        }

        public virtual AstNode VisitArrayLength(ArrayLength ast)
        {
            return ast;
        }

        public virtual AstNode VisitArrayLookup(ArrayLookup ast)
        {
            return ast;
        }

        public virtual AstNode VisitAssign(Assign ast)
        {
            return ast;
        }

        public virtual AstNode VisitBinary(Binary ast)
        {
            return ast;
        }

        public virtual AstNode VisitBlock(Block ast)
        {
            return ast;
        }

        public virtual AstNode VisitBooleanLiteral(BooleanLiteral ast)
        {
            return ast;
        }

        public virtual AstNode VisitBooleanType(BooleanType ast)
        {
            return ast;
        }

        public virtual AstNode VisitCall(Call ast)
        {
            return ast;
        }

        public virtual AstNode VisitClassDecl(ClassDecl ast)
        {
            return ast;
        }

        public virtual AstNode VisitFieldDecl(FieldDecl ast)
        {
            return ast;
        }

        public virtual AstNode VisitFormal(Formal ast)
        {
            return ast;
        }

        public virtual AstNode VisitIdentifierType(IdentifierType ast)
        {
            return ast;
        }

        public virtual AstNode VisitIfElse(IfElse ast)
        {
            return ast;
        }

        public virtual AstNode VisitIntArrayType(IntArrayType ast)
        {
            return ast;
        }

        public virtual AstNode VisitIntegerLiteral(IntegerLiteral ast)
        {
            return ast;
        }

        public virtual AstNode VisitIntegerType(IntegerType ast)
        {
            return ast;
        }

        public virtual AstNode VisitMainClass(MainClass ast)
        {
            return ast;
        }

        public virtual AstNode VisitMethodDecl(MethodDecl ast)
        {
            return ast;
        }

        public virtual AstNode VisitNewArray(NewArray ast)
        {
            return ast;
        }

        public virtual AstNode VisitNewObject(NewObject ast)
        {
            return ast;
        }

        public virtual AstNode VisitNot(Not ast)
        {
            return ast;
        }

        public virtual AstNode VisitProgram(Program ast)
        {
            return ast;
        }

        public virtual AstNode VisitThis(This ast)
        {
            return ast;
        }

        public virtual AstNode VisitVarDecl(VarDecl ast)
        {
            return ast;
        }

        public virtual AstNode VisitVariable(Variable ast)
        {
            return ast;
        }

        public virtual AstNode VisitWhile(While ast)
        {
            return ast;
        }

        public virtual AstNode VisitWriteLine(WriteLine ast)
        {
            return ast;
        }

        public virtual AstNode VisitTypeConvert(TypeConvert ast)
        {
            return ast;
        }
    }
}
