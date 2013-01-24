using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class Terminal : ProductionBase<Lexeme>
    {
        static Dictionary<Token, Terminal> s_tokonDict = new Dictionary<Token,Terminal>();

        public Token Token { get; private set; }

        private Terminal(Token token)
        {
            Token = token;
        }

        public static Terminal GetTerminal(Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            if (s_tokonDict.ContainsKey(token))
            {
                return s_tokonDict[token];
            }

            var newTerminal = new Terminal(token);
            s_tokonDict.Add(token, newTerminal);

            return newTerminal;
        }

        public override void Accept(IProductionVisitor visitor)
        {
            visitor.VisitTerminal(this);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Terminal;

            if (other == null)
            {
                return false;
            }

            return Token.Equals(other.Token);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Token.GetHashCode() + 1;
            }

        }
    }
}
