using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.MiniSharp.Ast;
using VBF.Compilers;
using System.Diagnostics;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp
{
    public class MethodBodyResolver : AstVisitor
    {
        private TypeCollection m_types;
        private VariableCollection<Parameter> m_currentMethodParameters;
        private VariableCollection<VariableInfo> m_currentMethodVariables;
        private CodeClassType m_currentType;
        private Method m_currentMethod;
        private int m_currentVariableIndex;

        private CompilationErrorManager m_errorManager;
        private const int c_SE_VariableDuplicates = 313;
        private const int c_SE_BinaryOpTypeInvalid = 320;
        private const int c_SE_UnaryOpTypeInvalid = 321;
        private const int c_SE_IfStmtTypeInvalid = 322;
        private const int c_SE_WhileStmtTypeInvalid = 323;
        private const int c_SE_InvalidCast = 324;
        private const int c_SE_WriteLineStmtTypeInvalid = 325;
        private const int c_SE_MethodMissing = 330;
        private const int c_SE_MethodInvalidArguments = 331;
        private const int c_SE_VariableDeclMissing = 332;
        private const int c_SE_NotArray = 333;
        private const int c_SE_ExpressionNotArray = 334;
        private const int c_SE_InvalidIntLiteral = 335;
        private const int c_SE_MethodAmbiguous = 336;
        private const int c_SE_ThisInStaticMethod = 337;
        private const int c_SE_NotSupported = 390;

        public void DefineErrors()
        {
            m_errorManager.DefineError(c_SE_VariableDuplicates, 0, CompilationStage.SemanticAnalysis,
                "There's already a parameter or local variable named '{0}' defined in current method.");

            m_errorManager.DefineError(c_SE_BinaryOpTypeInvalid, 0, CompilationStage.SemanticAnalysis,
                "Binary operator '{0}' cannot be used between '{1}' and '{2}'.");

            m_errorManager.DefineError(c_SE_UnaryOpTypeInvalid, 0, CompilationStage.SemanticAnalysis,
                "Unary operator '{0}' cannot be used on '{1}'.");

            m_errorManager.DefineError(c_SE_IfStmtTypeInvalid, 0, CompilationStage.SemanticAnalysis,
                "The type of expression used in if statement must be bool.");

            m_errorManager.DefineError(c_SE_WhileStmtTypeInvalid, 0, CompilationStage.SemanticAnalysis,
                "The type of expression used in while statement must be bool.");

            m_errorManager.DefineError(c_SE_WriteLineStmtTypeInvalid, 0, CompilationStage.SemanticAnalysis,
               "The System.Console.WriteLine statement can only output an integer value.");

            m_errorManager.DefineError(c_SE_InvalidCast, 0, CompilationStage.SemanticAnalysis,
                "Cannot cast from '{0}' to '{1}'.");

            m_errorManager.DefineError(c_SE_MethodMissing, 0, CompilationStage.SemanticAnalysis,
                "An applicable method '{0}' could not be found.");

            m_errorManager.DefineError(c_SE_MethodAmbiguous, 0, CompilationStage.SemanticAnalysis,
                "The call is ambiguous between the following two methods: '{0}' and '{1}'.");

            m_errorManager.DefineError(c_SE_VariableDeclMissing, 0, CompilationStage.SemanticAnalysis,
                "The name '{0}' is not declared.");

            m_errorManager.DefineError(c_SE_NotArray, 0, CompilationStage.SemanticAnalysis,
                "The name '{0}' is not an array.");

            m_errorManager.DefineError(c_SE_ExpressionNotArray, 0, CompilationStage.SemanticAnalysis,
                "The expression is not an array.");

            m_errorManager.DefineError(c_SE_MethodInvalidArguments, 0, CompilationStage.SemanticAnalysis,
                "The call to method '{0}' has some invalid arguments.");

            m_errorManager.DefineError(c_SE_ThisInStaticMethod, 0, CompilationStage.SemanticAnalysis,
                "The keyword 'this' cannot be used in a static method.");

            m_errorManager.DefineError(c_SE_NotSupported, 0, CompilationStage.SemanticAnalysis,
                "The usage is not support by miniSharp language: {0}");

            m_errorManager.DefineError(c_SE_InvalidIntLiteral, 0, CompilationStage.SemanticAnalysis,
                "'{0}' is not a valid integer.");
        }

        public MethodBodyResolver(CompilationErrorManager errorManager, TypeCollection types)
        {
            m_errorManager = errorManager;
            m_types = types;
        }

        private VariableInfo ResolveVariable(LexemeValue identifier)
        {
            //step1, check local parameter & variable definitions
            if (m_currentMethodParameters.Contains(identifier.Content))
            {
                return m_currentMethodParameters[identifier.Content];
            }
            else if (m_currentMethodVariables.Contains(identifier.Content))
            {
                return m_currentMethodVariables[identifier.Content];
            }
            //step2, if not static method, check fields
            if (!m_currentMethod.IsStatic)
            {
                return ResolveField(m_currentType, identifier);
            }

            m_errorManager.AddError(c_SE_VariableDeclMissing, identifier.Span, identifier.Content);
            return null;
        }

        private VariableInfo ResolveField(CodeClassType type, LexemeValue identifier)
        {
            //step1, see current class
            if (type.Fields.Contains(identifier.Content))
            {
                return type.Fields[identifier.Content];
            }

            //step2, see base class
            if (m_currentType.BaseType != null)
            {
                return ResolveField(m_currentType.BaseType, identifier);
            }

            m_errorManager.AddError(c_SE_VariableDeclMissing, identifier.Span, identifier.Content);
            return null;
        }

        private bool CheckVariableDecl(VarDecl ast)
        {
            //step1, check local parameter & variable definitions
            if (m_currentMethodParameters.Contains(ast.VariableName.Content))
            {
                m_errorManager.AddError(c_SE_VariableDuplicates, ast.VariableName.Span, ast.VariableName.Content);
                return false;
            }
            else if (m_currentMethodVariables.Contains(ast.VariableName.Content))
            {
                m_errorManager.AddError(c_SE_VariableDuplicates, ast.VariableName.Span, ast.VariableName.Content);
                return false;
            }

            //step2, resolve type
            ResolveTypeNode(ast.Type);
            return true;
        }

        private TypeBase ResolveTypeRef(TypeRef typeRef)
        {
            TypeBase resolvedType = PrimaryType.Unknown;
            var name = typeRef.TypeName;

            if (!m_types.Contains(name.Content))
            {
                m_errorManager.AddError(MemberDeclResolver.c_SE_TypeNameMissing, name.Span, name.Content);
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

            foreach (var c in ast.Classes)
            {
                Visit(c);
            }

            return ast;
        }

        public override AstNode VisitMainClass(MainClass ast)
        {
            m_currentType = ast.Type as CodeClassType;

            Debug.Assert(m_currentType.Methods.Count == 0);
            Debug.Assert(m_currentType.StaticMethods.Count == 1);
            m_currentMethod = m_currentType.StaticMethods[0];
            m_currentVariableIndex = 0;
            m_currentMethodParameters = new VariableCollection<Parameter>() { new Parameter() { Name = ast.ArgName.Content, Type = ArrayType.StrArray } };
            m_currentMethodVariables = new VariableCollection<VariableInfo>();

            foreach (var statement in ast.Statements)
            {
                Visit(statement);
            }

            return ast;
        }

        public override AstNode VisitClassDecl(ClassDecl ast)
        {
            m_currentType = ast.Type as CodeClassType;

            foreach (var method in ast.Methods)
            {
                Visit(method);
            }

            return ast;
        }

        public override AstNode VisitMethodDecl(MethodDecl ast)
        {
            m_currentMethod = ast.MethodInfo;
            m_currentVariableIndex = 0;
            m_currentMethodParameters = new VariableCollection<Parameter>();

            foreach (var param in m_currentMethod.Parameters)
            {
                m_currentMethodParameters.Add(param);
            }

            m_currentMethodVariables = new VariableCollection<VariableInfo>();

            if (ast.Statements == null || ast.ReturnExpression == null)
            {
                m_errorManager.AddError(c_SE_NotSupported, ast.Name.Span, "A method must have body defined");
                return ast;
            }

            foreach (var statement in ast.Statements)
            {
                Visit(statement);
            }

            Visit(ast.ReturnExpression);

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

        //resolve statements
        public override AstNode VisitBlock(Block ast)
        {
            m_currentMethodVariables.PushLevel();
            foreach (var statement in ast.Statements)
            {
                Visit(statement);
            }
            m_currentMethodVariables.PopLevel();

            return ast;
        }

        public override AstNode VisitIfElse(IfElse ast)
        {
            Visit(ast.Condition);

            if (ast.Condition.ExpressionType != PrimaryType.Boolean)
            {
                m_errorManager.AddError(c_SE_IfStmtTypeInvalid, ast.IfSpan);
            }

            Visit(ast.TruePart);
            Visit(ast.FalsePart);

            return ast;
        }

        public override AstNode VisitWhile(While ast)
        {

            Visit(ast.Condition);

            if (ast.Condition.ExpressionType != PrimaryType.Boolean)
            {
                m_errorManager.AddError(c_SE_WhileStmtTypeInvalid, ast.WhileSpan);
            }

            Visit(ast.LoopBody);

            return ast;
        }

        public override AstNode VisitWriteLine(WriteLine ast)
        {
            Visit(ast.Value);

            if (ast.Value.ExpressionType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_WriteLineStmtTypeInvalid, ast.WriteLineSpan);
            }

            return ast;
        }

        public override AstNode VisitAssign(Assign ast)
        {
            var variable = ResolveVariable(ast.Variable.VariableName);
            ast.Variable.VariableInfo = variable;

            Visit(ast.Value);

            if (variable == null)
            {
                //resolve failed
                return ast;
            }

            //check if assignable
            if (!variable.Type.IsAssignableFrom(ast.Value.ExpressionType))
            {
                m_errorManager.AddError(c_SE_InvalidCast, ast.Variable.VariableName.Span, ast.Value.ExpressionType.Name, variable.Type.Name);
            }

            if (variable.Type != ast.Value.ExpressionType)
            {
                var convert = new TypeConvert(ast.Value, variable.Type);
                ast.Value = convert;
            }

            return ast;
        }

        public override AstNode VisitArrayAssign(ArrayAssign ast)
        {
            var arrayVariable = ResolveVariable(ast.Array.VariableName);
            ast.Array.VariableInfo = arrayVariable;

            Visit(ast.Index);
            Visit(ast.Value);

            if (arrayVariable == null)
            {
                //resolve failed
                return ast;
            }

            ArrayType arrayType = arrayVariable.Type as ArrayType;
            if (arrayType == null)
            {
                m_errorManager.AddError(c_SE_NotArray, ast.Array.VariableName.Span, ast.Array.VariableName);
            }
            else if (arrayType.ElementType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_NotSupported, ast.Array.VariableName.Span, "Arrays rather than int[] are not operatable");
            }

            if (ast.Index.ExpressionType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_InvalidCast, ast.Array.VariableName.Span, ast.Index.ExpressionType.Name, PrimaryType.Int.Name);
            }

            if (ast.Value.ExpressionType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_InvalidCast, ast.Array.VariableName.Span, ast.Value.ExpressionType.Name, PrimaryType.Int.Name);
            }

            return ast;
        }

        public override AstNode VisitVarDecl(VarDecl ast)
        {
            if (CheckVariableDecl(ast))
            {
                //add to current variable table
                m_currentMethodVariables.Add(new VariableInfo() { Name = ast.VariableName.Content, Type = ast.Type.ResolvedType, Index = m_currentVariableIndex });
                ++m_currentVariableIndex;
            }

            return ast;
        }

        //resolve expressions

        public override AstNode VisitThis(This ast)
        {
            if (m_currentMethod.IsStatic)
            {
                m_errorManager.AddError(c_SE_ThisInStaticMethod, ast.ThisSpan);
            }
            ast.ExpressionType = m_currentType;
            return ast;
        }

        public override AstNode VisitNot(Not ast)
        {
            Visit(ast.Operand);

            if (ast.Operand.ExpressionType != PrimaryType.Boolean)
            {
                m_errorManager.AddError(c_SE_UnaryOpTypeInvalid, ast.OpSpan, "!", ast.Operand.ExpressionType.Name);
            }

            ast.ExpressionType = PrimaryType.Boolean;

            return ast;
        }

        public override AstNode VisitBooleanLiteral(BooleanLiteral ast)
        {
            ast.ExpressionType = PrimaryType.Boolean;
            return base.VisitBooleanLiteral(ast);
        }

        public override AstNode VisitIntegerLiteral(IntegerLiteral ast)
        {
            ast.ExpressionType = PrimaryType.Int;

            //check literal
            int value;
            if (!Int32.TryParse(ast.Literal.Content, out value))
            {
                m_errorManager.AddError(c_SE_InvalidIntLiteral, ast.Literal.Span, ast.Literal.Content);
            }
            else
            {
                ast.Value = value;
            }

            return base.VisitIntegerLiteral(ast);
        }

        public override AstNode VisitVariable(Variable ast)
        {
            var variable = ResolveVariable(ast.VariableRef.VariableName);
            ast.VariableRef.VariableInfo = variable;

            if (variable == null)
            {
                ast.ExpressionType = PrimaryType.Unknown;
            }
            else
            {
                ast.ExpressionType = variable.Type;
            }

            return ast;
        }

        public override AstNode VisitNewArray(NewArray ast)
        {
            Visit(ast.Length);

            if (ast.Length.ExpressionType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_InvalidCast, ast.LengthSpan, ast.Length.ExpressionType.Name, PrimaryType.Int.Name);
            }

            ast.ExpressionType = ArrayType.IntArray;

            return ast;
        }

        public override AstNode VisitNewObject(NewObject ast)
        {
            ast.ExpressionType = ResolveTypeRef(ast.Type);

            return ast;
        }

        public override AstNode VisitArrayLength(ArrayLength ast)
        {
            Visit(ast.Array);

            ArrayType arrayType = ast.Array.ExpressionType as ArrayType;
            if (arrayType == null)
            {
                m_errorManager.AddError(c_SE_ExpressionNotArray, ast.LengthSpan);
            }
            else if (arrayType.ElementType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_NotSupported, ast.LengthSpan, "Arrays rather than int[] are not operatable");
            }

            ast.ExpressionType = PrimaryType.Int;

            return ast;
        }

        public override AstNode VisitArrayLookup(ArrayLookup ast)
        {
            Visit(ast.Array);

            ArrayType arrayType = ast.Array.ExpressionType as ArrayType;
            if (arrayType == null)
            {
                m_errorManager.AddError(c_SE_ExpressionNotArray, ast.IndexSpan);
            }
            else if (arrayType.ElementType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_NotSupported, ast.IndexSpan, "Arrays rather than int[] are not operatable");
            }

            Visit(ast.Index);

            if (ast.Index.ExpressionType != PrimaryType.Int)
            {
                m_errorManager.AddError(c_SE_InvalidCast, ast.IndexSpan, ast.Index.ExpressionType.Name, PrimaryType.Int.Name);
            }

            ast.ExpressionType = arrayType == null ? PrimaryType.Unknown : arrayType.ElementType;

            return ast;
        }

        public override AstNode VisitBinary(Binary ast)
        {
            Visit(ast.Left);
            Visit(ast.Right);

            bool checkFailed = false;
            switch (ast.Operator)
            {
                case BinaryOperator.Add:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Int;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Int;

                    ast.ExpressionType = PrimaryType.Int;
                    break;
                case BinaryOperator.Substract:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Int;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Int;

                    ast.ExpressionType = PrimaryType.Int;
                    break;
                case BinaryOperator.Multiply:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Int;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Int;

                    ast.ExpressionType = PrimaryType.Int;
                    break;
                case BinaryOperator.Divide:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Int;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Int;

                    ast.ExpressionType = PrimaryType.Int;
                    break;
                case BinaryOperator.Less:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Int;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Int;

                    ast.ExpressionType = PrimaryType.Boolean;
                    break;
                case BinaryOperator.Greater:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Int;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Int;

                    ast.ExpressionType = PrimaryType.Boolean;
                    break;
                case BinaryOperator.Equal:
                    // use proper comparison instruction in translation stage
                    // == is allowed on:
                    // 1. class type and class type (compare ref)
                    // 2. array type and array type (compare ref)
                    // 3. int and int (compare value)
                    // 4. bool and bool (compare value)
                    checkFailed |= ast.Left.ExpressionType.GetType() != ast.Right.ExpressionType.GetType();
                    if (ast.Left.ExpressionType is PrimaryType && ast.Right.ExpressionType is PrimaryType)
                    {
                        checkFailed |= ast.Left.ExpressionType != ast.Right.ExpressionType;
                    }

                    ast.ExpressionType = PrimaryType.Boolean;
                    break;
                case BinaryOperator.LogicalAnd:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Boolean;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Boolean;

                    ast.ExpressionType = PrimaryType.Boolean;
                    break;
                case BinaryOperator.LogicalOr:
                    checkFailed |= ast.Left.ExpressionType != PrimaryType.Boolean;
                    checkFailed |= ast.Right.ExpressionType != PrimaryType.Boolean;

                    ast.ExpressionType = PrimaryType.Boolean;
                    break;
                default:
                    ast.ExpressionType = PrimaryType.Unknown;
                    break;
            }

            if (checkFailed)
            {
                m_errorManager.AddError(c_SE_BinaryOpTypeInvalid, ast.OpLexeme.Span, ast.OpLexeme.Content, ast.Left.ExpressionType.Name, ast.Right.ExpressionType.Name);
            }

            return ast;
        }

        public override AstNode VisitCall(Call ast)
        {
            // step 1. resolve each argument
            foreach (var argument in ast.Arguments)
            {
                Visit(argument);
            }

            //step 2. resolve object
            Visit(ast.Target);

            CodeClassType targetType = ast.Target.ExpressionType as CodeClassType;

            if (targetType == null)
            {
                m_errorManager.AddError(c_SE_MethodMissing, ast.Method.MethodName.Span, ast.Method.MethodName.Content);
                ast.ExpressionType = PrimaryType.Unknown;
                return ast;
            }

            //step 3. resolve method
            ResolveMethod(ast, targetType);

            return ast;
        }

        private void ResolveMethod(Call ast, CodeClassType targetType)
        {
            if (targetType == null)
            {
                m_errorManager.AddError(c_SE_MethodMissing, ast.Method.MethodName.Span, ast.Method.MethodName.Content);
                ast.ExpressionType = PrimaryType.Unknown;

                return;
            }

            // step 1: collect candidates from current type
            var candidates = (from m in targetType.Methods
                              where String.Equals(m.Name, ast.Method.MethodName.Content, StringComparison.InvariantCulture) 
                              && m.Parameters.Count == ast.Arguments.Count
                              select m).ToArray();

            if (candidates.Length == 0)
            {
                ResolveMethod(ast, targetType.BaseType);
                return;
            }

            // step 2: remove unqualifed candidates
            List<Method> qualifiedCandidates = new List<Method>();
            foreach (var candidate in candidates)
            {
                bool isQualified = true;
                for (int i = 0; i < candidate.Parameters.Count; i++)
                {
                    if (!candidate.Parameters[i].Type.IsAssignableFrom(ast.Arguments[i].ExpressionType))
                    {
                        isQualified = false;
                        break;
                    }
                }

                if (isQualified) qualifiedCandidates.Add(candidate);
            }

            if (qualifiedCandidates.Count == 0)
            {
                ResolveMethod(ast, targetType.BaseType);
                return;
            }

            // step 3: choose a "best" one
            if (qualifiedCandidates.Count > 1)
            {
                var comparer = new MethodOverloadingComparer(ast.Arguments);
                qualifiedCandidates.Sort(comparer);

                var firstCandidate = qualifiedCandidates[0];
                var secondCandidate = qualifiedCandidates[1];

                if (comparer.Compare(firstCandidate, secondCandidate) < 0)
                {
                    //choose first as the best one
                    ast.Method.MethodInfo = firstCandidate;
                    ast.ExpressionType = firstCandidate.ReturnType;
                }
                else
                {
                    //ambiguous between first & second
                    m_errorManager.AddError(c_SE_MethodAmbiguous, ast.Method.MethodName.Span, 
                        firstCandidate.GetSignatureString(), secondCandidate.GetSignatureString());
                    ast.ExpressionType = PrimaryType.Unknown;
                }
            }
            else
            {
                ast.Method.MethodInfo = qualifiedCandidates[0];
                ast.ExpressionType = qualifiedCandidates[0].ReturnType;
            }
        }
    }
}
