using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp.Ast
{
    public class Program : AstNode
    {
        public MainClass MainClass { get; set; }
        public ClassDecl[] Classes { get; set; }
    }
}
