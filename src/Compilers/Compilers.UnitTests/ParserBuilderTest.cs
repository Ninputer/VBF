using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers.Scanners;
using VBF.Compilers.Parsers;
using RE = VBF.Compilers.Scanners.RegularExpression;
using VBF.Compilers.Parsers.Generator;

namespace Compilers.UnitTests
{
    [TestFixture]
    public class ParserBuilderTest
    {
        [Test]
        public void ProductionInfoManagerTest()
        {
            Lexicon test = new Lexicon();

            var A = test.Lexer.DefineToken(RE.Symbol('a'));
            var D = test.Lexer.DefineToken(RE.Symbol('d'));
            var C = test.Lexer.DefineToken(RE.Symbol('c'));

            Production<object> X = new Production<object>(), Y = new Production<object>(), Z = new Production<object>();

            Z.Rule =
                (from d in D select d as object) |
                (from x in X
                 from y in Y
                 from z in Z
                 select new { x, y, z } as object);

            Y.Rule =
                Grammar.Empty(new object()) |
                (from c in C select c as object);

            X.Rule =
                Y |
                (from a in A select a as object);

            ProductionInfoManager pis = new ProductionInfoManager(Z);

            var xInfo = pis.GetInfo(X);
            var yInfo = pis.GetInfo(Y);
            var zInfo = pis.GetInfo(Z);

            Assert.IsTrue(xInfo.IsNullable, "X should be nullable");
            Assert.IsTrue(yInfo.IsNullable, "Y should be nullable");
            Assert.IsFalse(zInfo.IsNullable, "Z should not be nullable");

            Assert.AreEqual(xInfo.First.Count, 2);
            Assert.AreEqual(xInfo.Follow.Count, 3);

            Assert.IsTrue(xInfo.First.Contains(A.AsTerminal()));
            Assert.IsTrue(xInfo.First.Contains(C.AsTerminal()));

            Assert.IsTrue(xInfo.Follow.Contains(A.AsTerminal()));
            Assert.IsTrue(xInfo.Follow.Contains(C.AsTerminal()));
            Assert.IsTrue(xInfo.Follow.Contains(D.AsTerminal()));

            Assert.AreEqual(yInfo.First.Count, 1);
            Assert.AreEqual(yInfo.Follow.Count, 3);

            Assert.IsTrue(yInfo.First.Contains(C.AsTerminal()));

            Assert.IsTrue(yInfo.Follow.Contains(A.AsTerminal()));
            Assert.IsTrue(yInfo.Follow.Contains(C.AsTerminal()));
            Assert.IsTrue(yInfo.Follow.Contains(D.AsTerminal()));

            Assert.AreEqual(zInfo.First.Count, 3);
            Assert.AreEqual(zInfo.Follow.Count, 0);

            Assert.IsTrue(zInfo.First.Contains(A.AsTerminal()));
            Assert.IsTrue(zInfo.First.Contains(C.AsTerminal()));
            Assert.IsTrue(zInfo.First.Contains(D.AsTerminal()));
        }

        [Test]
        public void LR0Model_BuildModelTest()
        {
            Lexicon test = new Lexicon();

            var X = test.Lexer.DefineToken(RE.Symbol('x'));
            var PLUS = test.Lexer.DefineToken(RE.Symbol('+'));

            Production<object> E = new Production<object>(), T = new Production<object>();

            E.Rule =
                (from t in T
                 from plus in PLUS
                 from e in E
                 select new object()) | T;

            T.Rule =
                from x in X
                select new object();

            ProductionInfoManager pim = new ProductionInfoManager(E.SuffixedBy(Grammar.Eos()));

            LR0Model lr0 = new LR0Model(pim);
            lr0.BuildModel();

            var dot = lr0.ToString();

            ;
        }
    }
}
