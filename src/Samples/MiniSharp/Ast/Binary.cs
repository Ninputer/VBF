using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{

    public enum BinaryOperator
    {
        Add,
        Substract,
        Multiply,
        Divide,
        Less,
        Greater,
        Equal,
        LogicalAnd,
        LogicalOr
    }

    public class Binary : Expression
    {
        private static Dictionary<string, BinaryOperator> s_OperatorMap;

        static Binary()
        {
            s_OperatorMap = new Dictionary<string, BinaryOperator>();

            s_OperatorMap["+"] = BinaryOperator.Add;
            s_OperatorMap["-"] = BinaryOperator.Substract;
            s_OperatorMap["*"] = BinaryOperator.Multiply;
            s_OperatorMap["/"] = BinaryOperator.Divide;
            s_OperatorMap["<"] = BinaryOperator.Less;
            s_OperatorMap[">"] = BinaryOperator.Greater;
            s_OperatorMap["=="] = BinaryOperator.Equal;
            s_OperatorMap["&&"] = BinaryOperator.LogicalAnd;
            s_OperatorMap["||"] = BinaryOperator.LogicalOr;
        }

        public Expression Left { get; private set; }
        public Expression Right { get; private set; }
        public BinaryOperator Operator { get; private set; }
        public Lexeme OpLexeme { get; private set; }

        public Binary(Lexeme op, Expression left, Expression right)
        {
            Operator = s_OperatorMap[op.Value];
            Left = left;
            Right = right;
            OpLexeme = op;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }
}
