using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp.Ast
{
    public class MainClass : AstNode
    {
        public string Name { get; private set; }
        public string ArgName { get; private set; }
        public ReadOnlyCollection<Statement> Statements { get; private set; }

        public MainClass(string name, string argName, IList<Statement> statements)
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
