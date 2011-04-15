using VBF.Compilers.Scanners;
using RE = VBF.Compilers.Scanners.RegularExpression;
using NUnit.Framework;
using System;
using VBF.Compilers.Scanners.Generator;

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
            LexerState xml = lexicon.DefineLexerState();
            
            var ID = global.DefineToken(RE.Range('a', 'z').Sequence(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many()));
            var NUM = global.DefineToken(RE.Range('0', '9').Many1());
            var ERROR = global.DefineToken(RE.Range(Char.MinValue, (char)255));

            var IF = keywords.DefineToken(RE.Literal("if"));
            var ELSE = keywords.DefineToken(RE.Literal("else"));

            //var XMLNS = xml.DefineToken(RE.Literal("xmlns"));
            //NFAModel nfa = lexicon.CreateFiniteAutomatonModel();
            //DFAModel dfa = DFAModel.FromNFA(nfa);

            DFAModel dfa = DFAModel.Create(lexicon);

            CompressedTransitionTable tc = CompressedTransitionTable.Compress(dfa);
            
            ;
        }
    }
}
