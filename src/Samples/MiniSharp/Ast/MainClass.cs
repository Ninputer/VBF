using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using VBF.Compilers.Scanners;

namespace VBF.MiniSharp.Ast
{
    public class MainClass : AstNode
    {
        public Lexeme Name { get; private set; }
        public Lexeme ArgName { get; private set; }
        public ReadOnlyCollection<Statement> Statements { get; private set; }

        public TypeBase Type { get; set; }

        public MainClass(Lexeme name, Lexeme argName, IList<Statement> statements)
        {
            Name = name;
            ArgName = argName;
            Statements = new ReadOnlyCollection<Statement>(statements);
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitMainClass(this);
        }
    }
}
