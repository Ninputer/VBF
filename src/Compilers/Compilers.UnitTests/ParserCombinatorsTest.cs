using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VBF.Compilers.Parsers.Combinators;
using VBF.Compilers.Scanners;
using RE = VBF.Compilers.Scanners.RegularExpression;

namespace Compilers.UnitTests
{
    [TestFixture]
    public class ParserCombinatorsTest
    {
        [Test]
        public void ParserFuncTest()
        {
            Lexicon test = new Lexicon();

            var ID = test.DefaultLexer.DefineToken(RE.Range('a', 'z').Sequence(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many()));
            var NUM = test.DefaultLexer.DefineToken(RE.Range('0', '9').Many1());
            var WHITESPACE = test.DefaultLexer.DefineToken(RE.Symbol(' ').Union(RE.Symbol('\t')));

            var parser1 = from i in ID
                          from w in WHITESPACE
                          from n in NUM
                          select new { Id = i, Num = n };

            
        }
    }
}
