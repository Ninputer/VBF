using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class Terminal : Production<Lexeme>
    {
        public Token Token { get; private set; }

        public Terminal(Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            Token = token;
        }

        protected internal override void Accept<TResult>(IProductionVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
