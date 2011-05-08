using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public enum CorrectionMethod
    {
        Inserted,
        Deleted
    }

    public class ErrorCorrection
    {
        public CorrectionMethod Method { get; private set; }
        public string CorrectionToken { get; private set; }
        public Lexeme CorrectionLexeme { get; private set; }

        public ErrorCorrection(CorrectionMethod method, string correctionToken, Lexeme correctionLexeme)
        {
            Method = method;
            CorrectionToken = correctionToken;
            CorrectionLexeme = correctionLexeme;
        }
    }
}
