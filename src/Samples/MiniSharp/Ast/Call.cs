using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class Call : Expression
    {
        public Expression Target { get; private set; }
        public MethodRef Method { get; private set; }
        public ReadOnlyCollection<Expression> Arguments { get; private set; }

        public Call(Expression target, Lexeme methodName, IList<Expression> argList)
        {
            Target = target;
            Method = new MethodRef(methodName);
            Arguments = new ReadOnlyCollection<Expression>(argList);
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitCall(this);
        }
    }
}
