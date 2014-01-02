using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers
{
    public class EndOfStream : ProductionBase<Lexeme>
    {
        private static EndOfStream s_Instance = new EndOfStream();
        public static EndOfStream Instance
        {
            get
            {
                return s_Instance;
            }
        }

        private EndOfStream()
        {

        }

        public override TResult Accept<TArg, TResult>(IProductionVisitor<TArg, TResult> visitor, TArg argument)
        {
            return visitor.VisitEndOfStream(this, argument);
        }

        public override bool Equals(object obj)
        {
            var otherEos = obj as EndOfStream;

            if (otherEos != null)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return -3177;
        }

        public override bool IsTerminal
        {
            get
            {
                return true;
            }
        }

        public override bool IsEos
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
                return "$";
            }
        }

        public override string ToString()
        {
            return "$";
        }
    }
}
