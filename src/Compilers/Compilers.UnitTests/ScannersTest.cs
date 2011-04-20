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

            FiniteAutomationEngine engine = new FiniteAutomationEngine(lexicon.CreateScannerInfo());
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
            SourceReader source = new SourceReader(sr);

            while (!source.IsEndOfStream)
            {
                char value = (char)source.ReadChar();
                var location = source.Location;
            }
        }

        [Test]
        public void CacheQueueTest()
        {
            CacheQueue<int> q = new CacheQueue<int>();

            q.Enqueue(1);
            q.Enqueue(2);
            q.Enqueue(3);
            q.Enqueue(4);
            q.Enqueue(5);

            Assert.AreEqual(1, q.Dequeue());
            Assert.AreEqual(2, q.Dequeue());

            Assert.AreEqual(3, q.Count);

            q.Enqueue(6);
            q.Enqueue(7);
            q.Enqueue(8);
            q.Enqueue(9);

            Assert.AreEqual(7, q.Count);

            Assert.AreEqual(3, q[0]);
            Assert.AreEqual(4, q[1]);
            Assert.AreEqual(8, q[5]);
            Assert.AreEqual(9, q[6]);

            Assert.AreEqual(3, q.Dequeue());

            Assert.AreEqual(4, q[0]);
            Assert.AreEqual(9, q[5]);
            Assert.AreEqual(6, q.Count);

            q.Enqueue(10);
            q.Enqueue(11);

            Assert.AreEqual(11, q[7]);
            Assert.AreEqual(8, q.Count);
        }

        [Test]
        public void ScannerTest()
        {
            Lexicon lexicon = new Lexicon();
            LexerState global = lexicon.DefaultState;
            LexerState keywords = lexicon.DefineLexerState(global);
            LexerState xml = lexicon.DefineLexerState(keywords);

            var ID = global.DefineToken(RE.Range('a', 'z').Sequence(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many()));
            var NUM = global.DefineToken(RE.Range('0', '9').Many1());
            var WHITESPACE = global.DefineToken(RE.Symbol(' ').Many());
            var ERROR = global.DefineToken(RE.Range(Char.MinValue, (char)255));

            var IF = keywords.DefineToken(RE.Literal("if"));
            var ELSE = keywords.DefineToken(RE.Literal("else"));

            var XMLNS = xml.DefineToken(RE.Literal("xmlns"));

            Scanner scanner = new Scanner(lexicon.CreateScannerInfo());

            string source = "asdf04a 1107 else Z if vvv xmlns 772737";
            StringReader sr = new StringReader(source);

            scanner.SetSource(new SourceReader(sr));

            Lexeme l1 = scanner.Read();
            Assert.AreEqual(ID.Index,l1.TokenIdentityIndex);
            Assert.AreEqual("asdf04a", l1.Value);
            Assert.AreEqual(0, l1.Span.StartLocation.Column);
            Assert.AreEqual(6, l1.Span.EndLocation.Column);

            Lexeme l2 = scanner.Read();
            Lexeme l3 = scanner.Read();
            Lexeme l4 = scanner.Read();
            Lexeme l5 = scanner.Read();

            int p1 = scanner.Peek();
            int p2 = scanner.Peek2();
            int p3 = scanner.Peek(3);
            int p4 = scanner.Peek(4);
            int p5 = scanner.Peek(5);
        }
    }
}
