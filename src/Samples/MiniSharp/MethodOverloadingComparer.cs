using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.MiniSharp.Ast;
using System.Diagnostics;

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
