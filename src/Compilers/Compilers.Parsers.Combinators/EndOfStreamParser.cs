using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class EndOfStreamParser : Parser<Lexeme>
    {
        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<Lexeme, TFuture> future)
        {
            ParserFunc<TFuture> scan = null;
            scan = (scanner, context) =>
            {

                var l = scanner.Read();

                if (l.IsEndOfStream)
                {
                    return context.StepResult(0, () => future(l)(scanner, context));
                }
                else
                {
                    ErrorCorrection deleteCorrection = new DeletedErrorCorrection(l);
                    return context.StepResult(1, () => scan(scanner, context), deleteCorrection); //delete to recover
                }
            };

            return scan;
        }
    }
}
