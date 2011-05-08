using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class TokenParser : Parser<Lexeme>
    {
        public Token ExpectedToken { get; private set; }
        public string MissingCorrection { get; private set; }

        public TokenParser(Token expected, string missingCorrection)
        {
            CodeContract.RequiresArgumentNotNull(expected, "expected");

            ExpectedToken = expected;
            MissingCorrection = missingCorrection;
        }

        public TokenParser(Token expected)
        {
            CodeContract.RequiresArgumentNotNull(expected, "expected");

            ExpectedToken = expected;
            MissingCorrection = expected.ToString();
        }

        public override Func<ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<Lexeme, TFuture> future)
        {
            Func<ForkableScanner, ParserContext, Result<TFuture>> scan = null;
            scan = (scanner, context) =>
            {
                var s1 = scanner.Fork();

                var l = scanner.Read();
                if (l.TokenIndex == ExpectedToken.Index)
                {
                    s1.Close();
                    var r = context.StepResult(0, () => future(l)(scanner, context));
                    return r;
                }
                else
                {
                    Lexeme correctionLexeme = l.GetErrorCorrectionLexeme(ExpectedToken.Index, MissingCorrection);
                    ErrorCorrection insertCorrection = new ErrorCorrection(CorrectionMethod.Inserted, ExpectedToken.ToString(), correctionLexeme);

                    if (l.IsEndOfStream)
                    {
                        scanner.Join(s1);
                        return context.StepResult(1, () => future(correctionLexeme)(scanner, context), insertCorrection); //insert
                    }
                    else
                    {
                        ErrorCorrection deleteCorrection = new ErrorCorrection(CorrectionMethod.Deleted, ExpectedToken.ToString(), l);
                        return context.ChooseBest(context.StepResult(1, () => future(correctionLexeme)(s1, context), insertCorrection), //insert
                            context.StepResult(1, () => scan(scanner, context), deleteCorrection)); //delete
                    }
                }
            };

            return scan;
        }
    }
}
