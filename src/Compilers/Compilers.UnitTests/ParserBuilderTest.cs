using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Scanners;
using VBF.Compilers.Parsers;
using RE = VBF.Compilers.Scanners.RegularExpression;

namespace Compilers.UnitTests
{
    [TestFixture]
    public class ParserBuilderTest
    {
        [Test]
        public void ProductionInfoServiceTest()
        {
            Lexicon test = new Lexicon();

            var ID = test.Lexer.DefineToken(RE.Range('a', 'z').Concat(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many()));
            var NUM = test.Lexer.DefineToken(RE.Range('0', '9').Many1());
            var GREATER = test.Lexer.DefineToken(RE.Symbol('>'));

            var WHITESPACE = test.Lexer.DefineToken(RE.Symbol(' ').Union(RE.Symbol('\t')));

            Production<object> X = new Production<object>(), Y = new Production<object>(), Z = new Production<object>();
            Z.Rule = (from id in ID select id as object).Union(
                from x in X
                from y in Y
                from z in Z
                select new { x, y, z } as object);

            Y.Rule = Grammar.Empty(new object()).Union(
                from num in NUM select num as object);

            X.Rule = Y.Union(
                from greater in GREATER select greater as object);

            ProductionInfoServices pis = new ProductionInfoServices();

            pis.Visit(Z);

            ;
        }
    }
}
