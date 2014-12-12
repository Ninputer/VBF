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

using System.Collections.Generic;
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

        public Binary(LexemeValue op, Expression left, Expression right)
        {
            Operator = s_OperatorMap[op.Content];
            Left = left;
            Right = right;
            OpLexeme = op;
        }

        public Expression Left { get; private set; }
        public Expression Right { get; private set; }
        public BinaryOperator Operator { get; private set; }
        public LexemeValue OpLexeme { get; private set; }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitBinary(this);
        }
    }
}
