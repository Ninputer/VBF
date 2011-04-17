using VBF.Compilers.Scanners;
using RE = VBF.Compilers.Scanners.RegularExpression;
using NUnit.Framework;
using System;
using VBF.Compilers.Scanners.Generator;
using System.IO;

namespace Compilers.UnitTests
{


    /// <summary>
    ///This is a test class for RegularExpressionTest and is intended
    ///to contain all RegularExpressionTest Unit Tests
    ///</summary>
    [TestFixture]
    public class ScannersTest
    {

        [Test]
        public void RegExToDFATest()
        {
            //var RE_IF = RE.Literal("if");
            //var RE_ELSE = RE.Literal("else");
            var RE_ID = RE.Range('a', 'z').Sequence(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many());
            //var RE_NUM = RE.Range('0', '9').Many1();
            //var RE_ERROR = RE.Range(Char.MinValue, (char)255);
            Lexicon lexicon = new Lexicon();
            var ID = lexicon.DefaultState.DefineToken(RE_ID);

            NFAConverter nfaConverter = new NFAConverter();

            DFAModel D_ID = DFAModel.Create(lexicon);

            //verify state 0
            var state0 = D_ID.States[0];

            Assert.AreEqual(36, state0.OutEdges.Count);
            foreach (var edge in state0.OutEdges)
            {
                Assert.AreEqual(0, edge.TargetState.Index);
            }

            //verify initialization state
            var state1 = D_ID.States[1];

            foreach (var edge in state1.OutEdges)
            {
                if (edge.Symbol >= 'a' && edge.Symbol <= 'z')
                {
                    Assert.IsTrue(edge.TargetState.Index > 0);
                }
                else
                {
                    Assert.AreEqual(0, edge.TargetState.Index);
                }
            }

        }

        [Test]
        public void LexerStateToDFATest()
        {
            Lexicon lexicon = new Lexicon();
            LexerState global = lexicon.DefaultState;
            LexerState keywords = lexicon.DefineLexerState(global);
            LexerState xml = lexicon.DefineLexerState(keywords);

            var ID = global.DefineToken(RE.Range('a', 'z').Sequence(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many()));
            var NUM = global.DefineToken(RE.Range('0', '9').Many1());
            var ERROR = global.DefineToken(RE.Range(Char.MinValue, (char)255));

            var IF = keywords.DefineToken(RE.Literal("if"));
            var ELSE = keywords.DefineToken(RE.Literal("else"));

            var XMLNS = xml.DefineToken(RE.Literal("xmlns"));


            DFAModel dfa = DFAModel.Create(lexicon);

            CompressedTransitionTable tc = CompressedTransitionTable.Compress(dfa);

            FiniteAutomationEngine engine = FiniteAutomationEngine.CreateFromLexicon(lexicon);
            Assert.AreEqual(0, engine.CurrentLexerStateIndex);

            engine.InputString("if");

            Assert.IsTrue(engine.IsAtAcceptState);
            Assert.AreEqual(ID.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.InputString("12345");
            Assert.AreEqual(NUM.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.InputString("asdf12dd");
            Assert.AreEqual(ID.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.InputString("A");
            Assert.AreEqual(ERROR.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.InputString("AAA");
            Assert.IsFalse(engine.IsAtAcceptState);
            Assert.IsTrue(engine.IsAtStoppedState);

            engine.Reset();
            engine.InputString("if ");
            Assert.IsFalse(engine.IsAtAcceptState);
            Assert.IsTrue(engine.IsAtStoppedState);

            engine.Reset();
            engine.CurrentLexerStateIndex = keywords.Index;
            engine.InputString("if");
            Assert.AreEqual(IF.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.InputString("else");
            Assert.AreEqual(ELSE.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.InputString("xmlns");
            Assert.AreEqual(ID.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.CurrentLexerStateIndex = xml.Index;
            engine.InputString("if");
            Assert.IsTrue(engine.IsAtAcceptState);
            Assert.AreEqual(IF.Index, engine.CurrentTokenIndex);

            engine.Reset();
            engine.InputString("xml");
            Assert.IsFalse(engine.IsAtAcceptState);
            Assert.IsFalse(engine.IsAtStoppedState);

            engine.Reset();
            engine.InputString("xmlns");
            Assert.AreEqual(XMLNS.Index, engine.CurrentTokenIndex);
            ;
        }

        [Test]
        public void SourceCodeTest()
        {
            string code = @"class A
{
    public int c;
}";
            StringReader sr = new StringReader(code);
            SourceCode source = new SourceCode(sr);

            while (!source.IsEndOfStream)
            {
                char value = (char)source.ReadChar();
                var location = source.Location;
            }
        }
    }
}
