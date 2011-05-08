using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;
using System.Diagnostics;

namespace VBF.Compilers.Parsers.Combinators
{
    public class Parser<T>
    {
        public ParserFunc<T> Rule { get; private set; }

        public Parser()
        {

        }

        public Parser(ParserFunc<T> rule)
        {
            Rule = rule;
        }

        public void Assign<U>(Parser<U> assignedParser) where U : class, T
        {
            CodeContract.RequiresArgumentNotNull(assignedParser, "assignedParser");

            Rule = assignedParser.Rule;
        }

        public void Assign(Parser<T> assignedParser)
        {
            CodeContract.RequiresArgumentNotNull(assignedParser, "assignedParser");

            Rule = assignedParser.Rule;
        }

        public static Parser<T> operator |(Parser<T> leftParser, Parser<T> rightParser)
        {
            CodeContract.RequiresArgumentNotNull(leftParser, "leftParser");
            CodeContract.RequiresArgumentNotNull(rightParser, "rightParser");

            return leftParser.Union(rightParser);
        }
    }
}
