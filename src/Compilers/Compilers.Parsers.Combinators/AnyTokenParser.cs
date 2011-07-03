using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class AnyTokenParser : Parser<Lexeme>
    {
        public override ParserFunc<TFuture> BuildParser<TFuture>(Future<Lexeme, TFuture> future)
        {
            return (scanner, context) =>
            {
                var l = scanner.Read();
                return context.StepResult(0, () => future(l)(scanner, context));
            };
        }
    }
}
