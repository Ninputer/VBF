using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using VBF.MiniSharp.Ast;
using System.Diagnostics;

namespace VBF.MiniSharp.Targets.Cil
{
    public class EmitTranslator : AstVisitor
    {
        private readonly AssemblyBuilder m_assembly;
        private readonly ModuleBuilder m_module;
        private TypeBuilder m_currentType;
        private MethodBuilder m_currentMethod;
        private ILGenerator m_ilgen;

        private ExtensionTable<System.Type> m_typeTable;
        private ExtensionTable<MethodInfo> m_methodTable;
        private ExtensionTable<FieldInfo> m_fieldTable;

        public EmitTranslator(AppDomain hostDomain, string name)
        {
            AssemblyName asmName = new AssemblyName(name);
            m_assembly = hostDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave);

            m_module = m_assembly.DefineDynamicModule(name + ".dll", true);
            m_typeTable = new ExtensionTable<System.Type>();
            m_methodTable = new ExtensionTable<MethodInfo>();
            m_fieldTable = new ExtensionTable<FieldInfo>();
        }

        private System.Type GetClrType(TypeBase t)
        {
            var clrType = m_typeTable.Get(t);
            if (clrType != null)
            {
                return clrType;
            }

            CodeClassType ccType = t as CodeClassType;

            if (ccType != null)
            {
                System.Type baseClass = typeof(Object);

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



        public override AstNode VisitProgram(Program ast)
        {
            Visit(ast.MainClass);
            foreach (var c in ast.Classes)
            {
                Visit(c);
            }
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

            return ast;
        }

        public override AstNode VisitClassDecl(ClassDecl ast)
        {
            m_currentType = GetClrType(ast.Type) as TypeBuilder;

            foreach (var method in ast.Methods)
            {
                Visit(method);
            }

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

        private readonly MethodInfo m_WriteLine = typeof(Console).GetMethod("WriteLine", BindingFlags.Public | BindingFlags.Static, System.Type.DefaultBinder, new[] { typeof(int) }, null);
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


    }
}
