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
using System.Diagnostics;
using VBF.MiniSharp.Ast;

namespace VBF.MiniSharp
{
    public class MethodOverloadingComparer : IComparer<Method>
    {
        private IList<Expression> m_expressionList;

        public MethodOverloadingComparer(IList<Expression> expressions)
        {
            Debug.Assert(expressions != null);
            m_expressionList = expressions;
        }

        public int Compare(Method x, Method y)
        {
            //step 1. find one with better conversion.
            int lastComparisonResult = 0;
            for (int i = 0; i < m_expressionList.Count; i++)
            {
                int result = CompareConversion(x.Parameters[i].Type, y.Parameters[i].Type, m_expressionList[i]);

                if (lastComparisonResult < 0 && result > 0 || lastComparisonResult > 0 && result < 0)
                {
                    //none is better
                    return 0;
                }
                else if (result != 0)
                {
                    lastComparisonResult = result;
                }
            }

            return lastComparisonResult;
        }

        private int CompareConversion(TypeBase leftTarget, TypeBase rightTarget, Expression source)
        {
            if (leftTarget == rightTarget)
            {
                //same type, no better one
                return 0;
            }
            else if (leftTarget == source.ExpressionType && rightTarget != source.ExpressionType)
            {
                //left is better;
                return -1;
            }
            else if (leftTarget != source.ExpressionType && rightTarget == source.ExpressionType)
            {
                //right is better;
                return 1;
            }
            else
            {
                if (leftTarget.IsAssignableFrom(rightTarget))
                {
                    //right is more specific
                    return 1;
                }
                else if(rightTarget.IsAssignableFrom(leftTarget))
                {
                    //left is more specific
                    return -1;
                }
                else
                {
                    //both are bad
                    return 0;
                }
            }
        }
    }
}
