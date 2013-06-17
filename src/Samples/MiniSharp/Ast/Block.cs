using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace VBF.MiniSharp.Ast
{
    public class Block : Statement
    {
        public ReadOnlyCollection<Statement> Statements { get; private set; }

        public Block(IList<Statement> statements)
        {
            if (statements == null)
            {
                return;
            }
            Statements = new ReadOnlyCollection<Statement>(statements);
        }

        public override T Accept<T>(IAstVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
