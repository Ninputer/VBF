using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp.Ast
{
    public class MethodDecl : AstNode
    {
        public Method MethodInfo { get; set; }
        public Type ReturnType { get; private set; }
        public LexemeValue Name { get; private set; }
        public ReadOnlyCollection<Formal> Parameters { get; private set; }
        public ReadOnlyCollection<Statement> Statements { get; private set; }
        public Expression ReturnExpression { get; private set; }

        public MethodDecl(LexemeValue name, Type retType, IList<Formal> parameters, IList<Statement> stmts, Expression retExp)
        {
            Name = name;
            ReturnType = retType;
            Parameters = new ReadOnlyCollection<Formal>(parameters);
            Statements = stmts == null ? null : new ReadOnlyCollection<Statement>(stmts);
            ReturnExpression = retExp;
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitMethodDecl(this);
        }
    }
}
