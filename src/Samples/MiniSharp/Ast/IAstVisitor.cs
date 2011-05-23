using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public interface IAstVisitor<T>
    {
        T VisitArrayAssign(ArrayAssign ast);
        T VisitArrayLength(ArrayLength ast);
        T VisitArrayLookup(ArrayLookup ast);
        T VisitAssign(Assign ast);
        T VisitBinary(Binary ast);
        T VisitBlock(Block ast);
        T VisitBooleanLiteral(BooleanLiteral ast);
        T VisitBooleanType(BooleanType ast);
        T VisitCall(Call ast);
        T VisitClassDecl(ClassDecl ast);
        T VisitFieldDecl(FieldDecl ast);
        T VisitFormal(Formal ast);
        T VisitIdentifierType(IdentifierType ast);
        T VisitIfElse(IfElse ast);
        T VisitIntArrayType(IntArrayType ast);
        T VisitIntegerLiteral(IntegerLiteral ast);
        T VisitIntegerType(IntegerType ast);
        T VisitMainClass(MainClass ast);
        T VisitMethodDecl(MethodDecl ast);
        T VisitNewArray(NewArray ast);
        T VisitNewObject(NewObject ast);
        T VisitNot(Not ast);
        T VisitProgram(Program ast);
        T VisitThis(This ast);
        T VisitVarDecl(VarDecl ast);
        T VisitVariable(Variable ast);
        T VisitWhile(While ast);
        T VisitWriteLine(WriteLine ast);
    }
}
