using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp.Ast
{
    public class ClassDecl : AstNode
    {
        public TypeRef BaseClass { get; private set; }
        public LexemeValue Name { get; private set; }
        public ReadOnlyCollection<FieldDecl> Fields { get; private set; }
        public ReadOnlyCollection<MethodDecl> Methods { get; private set; }

        public TypeBase Type { get; set; }

        public ClassDecl(LexemeValue name, LexemeValue baseClassName, IList<FieldDecl> fields, IList<MethodDecl> methods)
        {
            BaseClass = new TypeRef(baseClassName);
            Name = name;
            Fields = new ReadOnlyCollection<FieldDecl>(fields);
            Methods = new ReadOnlyCollection<MethodDecl>(methods);
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitClassDecl(this);
        }
    }
}
