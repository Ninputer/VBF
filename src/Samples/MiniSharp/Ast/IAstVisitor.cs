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
        T VisitTypeConvert(TypeConvert ast);
        T VisitVarDecl(VarDecl ast);
        T VisitVariable(Variable ast);
        T VisitWhile(While ast);
        T VisitWriteLine(WriteLine ast);
    }
}
