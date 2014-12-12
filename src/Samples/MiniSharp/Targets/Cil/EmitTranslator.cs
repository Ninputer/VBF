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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using VBF.MiniSharp.Ast;
using Type = System.Type;

namespace VBF.MiniSharp.Targets.Cil
{
    public class EmitTranslator : AstVisitor
    {
        private static readonly MethodInfo m_WriteLine = typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder, new[] { typeof(int) }, null);
        private static readonly MethodInfo ArrayGetLengthMethod = typeof(int[]).GetProperty("Length").GetGetMethod();
        private readonly AssemblyBuilder m_assembly;
        private readonly ModuleBuilder m_module;
        private ExtensionTable<ConstructorInfo> m_ctorTable;
        private MethodBuilder m_currentMethod;
        private TypeBuilder m_currentType;
        private ExtensionTable<FieldInfo> m_fieldTable;
        private ILGenerator m_ilgen;

        private MethodInfo m_mainMethod;

        private ExtensionTable<MethodInfo> m_methodTable;
        private ExtensionTable<Type> m_typeTable;

        public EmitTranslator(AppDomain hostDomain, string name)
        {
            AssemblyName asmName = new AssemblyName(name);
            m_assembly = hostDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);

            m_module = m_assembly.DefineDynamicModule(name + ".exe", true);
            m_typeTable = new ExtensionTable<Type>();
            m_methodTable = new ExtensionTable<MethodInfo>();
            m_ctorTable = new ExtensionTable<ConstructorInfo>();
            m_fieldTable = new ExtensionTable<FieldInfo>();
        }

        public void Create(AstNode ast, string url)
        {
            Visit(ast);

            Debug.Assert(m_assembly != null);

            m_assembly.SetEntryPoint(m_mainMethod, PEFileKinds.ConsoleApplication);
            m_assembly.Save(url);
        }

        private Type GetClrType(TypeBase t)
        {
            var clrType = m_typeTable.Get(t);
            if (clrType != null)
            {
                return clrType;
            }

            CodeClassType ccType = t as CodeClassType;

            if (ccType != null)
            {
                Type baseClass = typeof(Object);

                if (ccType.BaseType != null)
                {
                    baseClass = GetClrType(ccType.BaseType);
                }

                clrType = m_module.DefineType(t.Name, TypeAttributes.Class | TypeAttributes.BeforeFieldInit, baseClass);

                m_typeTable.Set(t, clrType);
                return clrType;
            }

            ArrayType aType = t as ArrayType;
            if (aType != null)
            {
                var elementType = GetClrType(aType.ElementType);

                clrType = elementType.MakeArrayType();
                m_typeTable.Set(t, clrType);

                return clrType;
            }

            PrimaryType pType = t as PrimaryType;

            switch (pType.Name)
            {
                case "int":
                    clrType = typeof(int);
                    break;
                case "bool":
                    clrType = typeof(bool);
                    break;
                default:
                    Debug.Assert(false, "unknown primary type");
                    break;
            }

            m_typeTable.Set(t, clrType);
            return clrType;

        }

        private MethodInfo GetClrMethod(Method m)
        {
            var mi = m_methodTable.Get(m);
            if (mi != null)
            {
                return mi;
            }

            var declType = GetClrType(m.DeclaringType) as TypeBuilder;
            var returnType = GetClrType(m.ReturnType);

            var paramTypes = m.Parameters.Select(p => GetClrType(p.Type)).ToArray();

            MethodBuilder mb = declType.DefineMethod(
                m.Name,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                returnType,
                paramTypes);

            foreach (var param in m.Parameters)
            {
                mb.DefineParameter(param.Index, ParameterAttributes.None, param.Name);
            }

            m_methodTable.Set(m, mb);
            return mb;
        }

        private FieldInfo GetClrField(Field f)
        {
            var fi = m_fieldTable.Get(f);
            if (fi != null)
            {
                return fi;
            }

            var declType = GetClrType(f.DeclaringType) as TypeBuilder;
            var fieldType = GetClrType(f.Type);

            FieldBuilder fb = declType.DefineField(
                f.Name,
                fieldType,
                FieldAttributes.Private);

            m_fieldTable.Set(f, fb);
            return fb;
        }

        private ConstructorInfo GetClrCtor(TypeBase t)
        {
            var ctor = m_ctorTable.Get(t);
            if (ctor != null)
            {
                return ctor;
            }

            var codeType = t as CodeClassType;
            if (codeType == null || codeType.IsStatic)
            {
                return null;
            }

            ConstructorInfo baseCtor;
            if (codeType.BaseType != null)
            {
                baseCtor = GetClrType(codeType.BaseType).GetConstructor(new Type[0]);
            }
            else
            {
                baseCtor = typeof(Object).GetConstructor(new Type[0]);
            }

            TypeBuilder type = GetClrType(t) as TypeBuilder;

            const MethodAttributes ctorAttr = MethodAttributes.Public | MethodAttributes.PrivateScope |
                MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;


            var ctorb = type.DefineConstructor(ctorAttr, CallingConventions.Standard, new Type[0]);
            var il = ctorb.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, baseCtor);
            il.Emit(OpCodes.Ret);

            ctor = ctorb;

            m_ctorTable.Set(t, ctor);
            return ctor;
        }

        public override AstNode VisitProgram(Program ast)
        {
            List<ClassDecl> classesInHierarchyOrder = new List<ClassDecl>();

            var topBaseClasses = from c in ast.Classes where c.BaseClass.Type == null select c;
            classesInHierarchyOrder.AddRange(topBaseClasses);

            while (classesInHierarchyOrder.Count < ast.Classes.Count)
            {
                foreach (var c in ast.Classes)
                {
                    foreach (var b in classesInHierarchyOrder.ToArray())
                    {
                        if (c.BaseClass.Type == b.Type)
                        {
                            classesInHierarchyOrder.Add(c);
                        }
                    }
                }
            }

            foreach (var c in classesInHierarchyOrder)
            {
                Visit(c);
            }

            Visit(ast.MainClass);

            return ast;
        }

        public override AstNode VisitMainClass(MainClass ast)
        {
            m_currentType = m_module.DefineType(ast.Type.Name, TypeAttributes.Class | TypeAttributes.Abstract | TypeAttributes.Sealed);
            m_currentMethod = m_currentType.DefineMethod("Main", MethodAttributes.Public | MethodAttributes.Static, typeof(void), new[] { typeof(string[]) });

            m_ilgen = m_currentMethod.GetILGenerator();

            foreach (var s in ast.Statements)
            {
                Visit(s);
            }

            m_ilgen.Emit(OpCodes.Ret);

            m_currentType.CreateType();
            m_mainMethod = m_currentMethod;
            return ast;
        }

        public override AstNode VisitClassDecl(ClassDecl ast)
        {
            m_currentType = GetClrType(ast.Type) as TypeBuilder;

            foreach (var method in ast.Methods)
            {
                Visit(method);
            }

            GetClrCtor(ast.Type);

            m_currentType.CreateType();

            return ast;
        }

        public override AstNode VisitMethodDecl(MethodDecl ast)
        {
            m_currentMethod = GetClrMethod(ast.MethodInfo) as MethodBuilder;
            m_ilgen = m_currentMethod.GetILGenerator();

            foreach (var s in ast.Statements)
            {
                Visit(s);
            }

            //generates return expression
            Visit(ast.ReturnExpression);
            m_ilgen.Emit(OpCodes.Ret);

            return ast;
        }

        //translate statements


        public override AstNode VisitBlock(Block ast)
        {
            m_ilgen.BeginScope();

            foreach (var s in ast.Statements)
            {
                Visit(s);
            }

            m_ilgen.EndScope();
            return ast;
        }

        public override AstNode VisitIfElse(IfElse ast)
        {
            var ifBlock = m_ilgen.DefineLabel();
            var elseBlock = m_ilgen.DefineLabel();
            var endif = m_ilgen.DefineLabel();

            Visit(ast.Condition);
            //the e-stack should have a bool value
            m_ilgen.Emit(OpCodes.Brfalse, elseBlock);

            //if block
            m_ilgen.MarkLabel(ifBlock);
            Visit(ast.TruePart);
            m_ilgen.Emit(OpCodes.Br, endif);

            //elseblock
            m_ilgen.MarkLabel(elseBlock);
            Visit(ast.FalsePart);

            //after if
            m_ilgen.MarkLabel(endif);

            return ast;
        }

        public override AstNode VisitWhile(While ast)
        {
            var beforeWhile = m_ilgen.DefineLabel();
            var afterWhile = m_ilgen.DefineLabel();

            m_ilgen.MarkLabel(beforeWhile);

            Visit(ast.Condition);
            //the e-stack should have a bool value
            m_ilgen.Emit(OpCodes.Brfalse, afterWhile);

            Visit(ast.LoopBody);

            m_ilgen.Emit(OpCodes.Br, beforeWhile);
            m_ilgen.MarkLabel(afterWhile);

            return ast;
        }

        public override AstNode VisitWriteLine(WriteLine ast)
        {
            //push argument to e-stack
            Visit(ast.Value);

            m_ilgen.EmitCall(OpCodes.Call, m_WriteLine, null);

            return ast;
        }

        public override AstNode VisitVarDecl(VarDecl ast)
        {
            var type = GetClrType(ast.Type.ResolvedType);
            m_ilgen.DeclareLocal(type);

            return ast;
        }

        private void EmitSetLocal(int locIndex)
        {
            switch (locIndex)
            {
                case 0:
                    m_ilgen.Emit(OpCodes.Stloc_0);
                    break;
                case 1:
                    m_ilgen.Emit(OpCodes.Stloc_1);
                    break;
                case 2:
                    m_ilgen.Emit(OpCodes.Stloc_2);
                    break;
                case 3:
                    m_ilgen.Emit(OpCodes.Stloc_3);
                    break;
                default:
                    if (locIndex <= 255)
                    {
                        m_ilgen.Emit(OpCodes.Stloc_S, (byte)locIndex);
                    }
                    else
                    {
                        m_ilgen.Emit(OpCodes.Stloc, (short)locIndex);
                    }
                    break;
            }
        }

        private void EmitSetArg(int argIndex)
        {
            if (argIndex <= 255)
            {
                m_ilgen.Emit(OpCodes.Starg_S, (byte)argIndex);
            }
            else
            {
                m_ilgen.Emit(OpCodes.Starg, (short)argIndex);
            }
        }

        public override AstNode VisitAssign(Assign ast)
        {
            var vi = ast.Variable.VariableInfo;

            Field f = vi as Field;

            if (f != null)
            {
                FieldInfo fi = GetClrField(f);

                //load "this"
                m_ilgen.Emit(OpCodes.Ldarg_0);

                //push value to e-stack
                Visit(ast.Value);

                m_ilgen.Emit(OpCodes.Stfld, fi);
                return ast;
            }

            //push value to e-stack
            Visit(ast.Value);

            Parameter p = vi as Parameter;

            if (p != null)
            {
                EmitSetArg(p.Index);
                return ast;
            }

            //local variable
            EmitSetLocal(vi.Index);
            return ast;
        }

        private void EmitLoadLocal(int locIndex)
        {
            switch (locIndex)
            {
                case 0:
                    m_ilgen.Emit(OpCodes.Ldloc_0);
                    break;
                case 1:
                    m_ilgen.Emit(OpCodes.Ldloc_1);
                    break;
                case 2:
                    m_ilgen.Emit(OpCodes.Ldloc_2);
                    break;
                case 3:
                    m_ilgen.Emit(OpCodes.Ldloc_3);
                    break;
                default:
                    if (locIndex <= 255)
                    {
                        m_ilgen.Emit(OpCodes.Ldloc_S, (byte)locIndex);
                    }
                    else
                    {
                        m_ilgen.Emit(OpCodes.Ldloc, (short)locIndex);
                    }
                    break;
            }
        }

        private void EmitLoadArgument(int argIndex)
        {
            switch (argIndex)
            {
                case 0:
                    m_ilgen.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    m_ilgen.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    m_ilgen.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    m_ilgen.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (argIndex <= 255)
                    {
                        m_ilgen.Emit(OpCodes.Ldarg_S, (byte)argIndex);
                    }
                    else
                    {
                        m_ilgen.Emit(OpCodes.Ldarg, (short)argIndex);
                    }
                    break;
            }
        }

        public override AstNode VisitArrayAssign(ArrayAssign ast)
        {
            //push array object
            var vi = ast.Array.VariableInfo;

            Field f = vi as Field;
            Parameter p = vi as Parameter;

            if (f != null)
            {
                FieldInfo fi = GetClrField(f);

                //load "this"
                m_ilgen.Emit(OpCodes.Ldarg_0);
                //load field
                m_ilgen.Emit(OpCodes.Ldfld, fi);

            }
            else if (p != null)
            {
                EmitLoadArgument(p.Index);
            }
            else
            {
                EmitLoadLocal(vi.Index);
            }

            //push index to e-stack
            Visit(ast.Index);

            //push value to e-stack
            Visit(ast.Value);

            m_ilgen.Emit(OpCodes.Stelem_I4);
            return ast;
        }

        //translate expressions

        public override AstNode VisitThis(This ast)
        {
            //push "this
            m_ilgen.Emit(OpCodes.Ldarg_0);
            return ast;
        }

        public override AstNode VisitTypeConvert(TypeConvert ast)
        {
            Visit(ast.Source);

            var targetType = GetClrType(ast.ExpressionType);

            m_ilgen.Emit(OpCodes.Castclass, targetType);

            return ast;
        }

        public override AstNode VisitVariable(Variable ast)
        {
            var vi = ast.VariableRef.VariableInfo;

            Field f = vi as Field;
            Parameter p = vi as Parameter;

            if (f != null)
            {
                FieldInfo fi = GetClrField(f);

                //load "this"
                m_ilgen.Emit(OpCodes.Ldarg_0);
                //load field
                m_ilgen.Emit(OpCodes.Ldfld, fi);

            }
            else if (p != null)
            {
                EmitLoadArgument(p.Index);
            }
            else
            {
                EmitLoadLocal(vi.Index);
            }

            return ast;
        }

        public override AstNode VisitBooleanLiteral(BooleanLiteral ast)
        {
            if (ast.Value)
            {
                m_ilgen.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                m_ilgen.Emit(OpCodes.Ldc_I4_0);
            }
            return ast;
        }

        public override AstNode VisitIntegerLiteral(IntegerLiteral ast)
        {
            switch (ast.Value)
            {
                case -1:
                    m_ilgen.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    m_ilgen.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    m_ilgen.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    m_ilgen.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    m_ilgen.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    m_ilgen.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    m_ilgen.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    m_ilgen.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    m_ilgen.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    m_ilgen.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (ast.Value > 8 || ast.Value <= 127)
                    {
                        m_ilgen.Emit(OpCodes.Ldc_I4_S, (byte)ast.Value);
                    }
                    else
                    {
                        m_ilgen.Emit(OpCodes.Ldc_I4, ast.Value);
                    }
                    break;
            }
            return ast;
        }

        public override AstNode VisitNot(Not ast)
        {
            Visit(ast.Operand);

            m_ilgen.Emit(OpCodes.Ldc_I4_0);
            m_ilgen.Emit(OpCodes.Ceq);

            return ast;
        }

        public override AstNode VisitArrayLength(ArrayLength ast)
        {
            //push object to e-stack
            Visit(ast.Array);

            m_ilgen.EmitCall(OpCodes.Callvirt, ArrayGetLengthMethod, null);

            return ast;
        }

        public override AstNode VisitArrayLookup(ArrayLookup ast)
        {
            //push array to e-stack
            Visit(ast.Array);

            //push index
            Visit(ast.Index);

            m_ilgen.Emit(OpCodes.Ldelem_I4);

            return ast;
        }

        public override AstNode VisitBinary(Binary ast)
        {
            //push operands
            Visit(ast.Left);
            Visit(ast.Right);

            switch (ast.Operator)
            {
                case BinaryOperator.Add:
                    m_ilgen.Emit(OpCodes.Add);
                    break;
                case BinaryOperator.Substract:
                    m_ilgen.Emit(OpCodes.Sub);
                    break;
                case BinaryOperator.Multiply:
                    m_ilgen.Emit(OpCodes.Mul);
                    break;
                case BinaryOperator.Divide:
                    m_ilgen.Emit(OpCodes.Div);
                    break;
                case BinaryOperator.Less:
                    m_ilgen.Emit(OpCodes.Clt);
                    break;
                case BinaryOperator.Greater:
                    m_ilgen.Emit(OpCodes.Cgt);
                    break;
                case BinaryOperator.Equal:
                    m_ilgen.Emit(OpCodes.Ceq);
                    break;
                case BinaryOperator.LogicalAnd:
                    m_ilgen.Emit(OpCodes.And);
                    break;
                case BinaryOperator.LogicalOr:
                    m_ilgen.Emit(OpCodes.Or);
                    break;
                default:
                    m_ilgen.Emit(OpCodes.Pop);
                    m_ilgen.Emit(OpCodes.Pop);
                    m_ilgen.Emit(OpCodes.Ldc_I4_0);
                    break;
            }
            return ast;
        }

        public override AstNode VisitCall(Call ast)
        {
            var methodRInfo = GetClrMethod(ast.Method.MethodInfo);

            //push target object
            Visit(ast.Target);

            //push arguments
            foreach (var arg in ast.Arguments)
            {
                Visit(arg);
            }

            m_ilgen.EmitCall(OpCodes.Call, methodRInfo, null);

            return ast;
        }

        public override AstNode VisitNewObject(NewObject ast)
        {
            var ctor = GetClrCtor(ast.Type.Type);

            m_ilgen.Emit(OpCodes.Newobj, ctor);

            return ast;
        }

        public override AstNode VisitNewArray(NewArray ast)
        {
            Visit(ast.Length);

            var elementType = typeof(int);

            m_ilgen.Emit(OpCodes.Newarr, elementType);

            return ast;
        }
    }
}
