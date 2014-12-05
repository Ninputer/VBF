// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class Terminal : ProductionBase<Lexeme>
    {
        //static Dictionary<Token, Terminal> s_tokenDict = new Dictionary<Token,Terminal>();

        private Terminal(Token token)
        {
            Token = token;
        }

        public Token Token { get; private set; }

        public override bool IsTerminal
        {
            get
            {
                return true;
            }
        }

        public override string DebugName
        {
            get
            {
                return Token.Description;
            }
        }

        public static Terminal GetTerminal(Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            /*
            Terminal terminal = null;


            if (s_tokenDict.TryGetValue(token, out terminal))
            {
                return terminal;
            }

            terminal = new Terminal(token);
            s_tokenDict.Add(token, terminal);

            return terminal;
            //*/

            return new Terminal(token);
        }

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument)
        {
            return visitor.VisitTerminal(this, argument);
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

        public override string ToString()
        {
            return Token.Description;
        }
    }
}
