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
using System.IO;
using VBF.Compilers;

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
        public void ParserDriverConflictTest()
        {
            Lexicon test = new Lexicon();

            var X = test.Lexer.DefineToken(RE.Symbol('x'));
            var PLUS = test.Lexer.DefineToken(RE.Symbol('+'));
            var ASTERISK = test.Lexer.DefineToken(RE.Symbol('*'));

            var scannerinfo = test.CreateScannerInfo();

            Production<object> E = new Production<object>(), T = new Production<object>();

            E.Rule =
                (from e1 in E
                 from plus in PLUS
                 from e2 in E
                 select (object)(((int)e1) + ((int)e2))) |
                (from e1 in E
                 from mul in ASTERISK
                 from e2 in E
                 select (object)(((int)e1) * ((int)e2))) | T;

            T.Rule =
                from x in X
                select (object)2;

            ProductionInfoManager pim = new ProductionInfoManager(E.SuffixedBy(Grammar.Eos()));

            LR0Model lr0 = new LR0Model(pim);
            lr0.BuildModel();

            string dot = lr0.ToString();

            TransitionTable tt = TransitionTable.Create(lr0, scannerinfo);

            ParserEngine driver = new ParserEngine(tt, new SyntaxErrors());

            ForkableScannerBuilder builder = new ForkableScannerBuilder(scannerinfo);

            var em = new VBF.Compilers.CompilationErrorManager();
            var el = em.CreateErrorList();

            builder.ErrorList = el;
            var scanner = builder.Create(new VBF.Compilers.SourceReader(new StringReader("x+x*x")));

            var z1 = scanner.Read();

            driver.Input(z1);

            var z2 = scanner.Read();

            driver.Input(z2);

            var z3 = scanner.Read();

            driver.Input(z3);

            var z4 = scanner.Read();

            driver.Input(z4);

            var z5 = scanner.Read();

            driver.Input(z5);

            var z6 = scanner.Read();

            driver.Input(z6);

            Assert.AreEqual(0, driver.CurrentStackCount);
            Assert.AreEqual(2, driver.AcceptedCount);

            var results = new[] { (int)driver.GetResult(0, null), (int)driver.GetResult(1, null) };

            Assert.IsTrue(results.Contains(8));
            Assert.IsTrue(results.Contains(6));

        }

        [Test]
        public void ParserDriverSimpleTest()
        {
            Lexicon test = new Lexicon();

            var X = test.Lexer.DefineToken(RE.Symbol('x'));
            var PLUS = test.Lexer.DefineToken(RE.Symbol('+'));

            var scannerinfo = test.CreateScannerInfo();

            Production<object> E = new Production<object>(), T = new Production<object>();

            E.Rule =
                (from t in T
                 from plus in PLUS
                 from e in E
                 select (object)(((int)t) + ((int)e))) | T;

            T.Rule =
                from x in X
                select (object)1;

            ProductionInfoManager pim = new ProductionInfoManager(E.SuffixedBy(Grammar.Eos()));

            LR0Model lr0 = new LR0Model(pim);
            lr0.BuildModel();

            string dot = lr0.ToString();

            TransitionTable tt = TransitionTable.Create(lr0, scannerinfo);

            ParserEngine driver = new ParserEngine(tt, new SyntaxErrors() { TokenUnexpectedId = 1 });

            ForkableScannerBuilder builder = new ForkableScannerBuilder(scannerinfo);
            var em = new VBF.Compilers.CompilationErrorManager();;
            var el = em.CreateErrorList();

            builder.ErrorList = el; 
            var scanner = builder.Create(new VBF.Compilers.SourceReader(new StringReader("x+x+x")));

            var z1 = scanner.Read();

            driver.Input(z1);

            var z2 = scanner.Read();

            driver.Input(z2);

            var z3 = scanner.Read();

            driver.Input(z3);

            var z4 = scanner.Read();

            driver.Input(z4);

            var z5 = scanner.Read();

            driver.Input(z5);

            var z6 = scanner.Read();

            driver.Input(z6);

            Assert.AreEqual(0, driver.CurrentStackCount);
            Assert.AreEqual(1, driver.AcceptedCount);
            Assert.AreEqual(3, driver.GetResult(0, null));
        }

        [Test]
        public void WhereGrammaTest()
        {
            Lexicon test = new Lexicon();

            var ID = test.Lexer.DefineToken(RE.Range('a', 'z').Concat(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many()), "ID");
            var NUM = test.Lexer.DefineToken(RE.Range('0', '9').Many1(), "NUM");
            var GREATER = test.Lexer.DefineToken(RE.Symbol('>'));

            var WHITESPACE = test.Lexer.DefineToken(RE.Symbol(' ').Union(RE.Symbol('\t')), "[ ]");

            var p1 = from i in ID
                     from g in GREATER
                     from g2 in GREATER
                     where Grammar.Check(g2.PrefixTrivia.Count == 0, 4, g2.Value.Span)
                     from n in NUM
                     select "A";

            var p2 = from i in ID
                     from g in GREATER
                     from g2 in GREATER
                     from n in NUM
                     select "B";

            var parser1 = p1 | p2;
            parser1.AmbiguityAggregator = (a, b) => a == "A" ? a : b;

            var info = test.CreateScannerInfo();

            var errorManager = new CompilationErrorManager();
            errorManager.DefineError(1, 0, CompilationStage.Parsing, "Unexpected token '{0}'");
            errorManager.DefineError(2, 0, CompilationStage.Parsing, "Missing token '{0}'");
            errorManager.DefineError(3, 0, CompilationStage.Parsing, "Syntax error");
            errorManager.DefineError(4, 0, CompilationStage.Parsing, "White spaces between >> are not allowed");

            var el = errorManager.CreateErrorList();

            ProductionInfoManager pim = new ProductionInfoManager(parser1.SuffixedBy(Grammar.Eos()));

            LR0Model lr0 = new LR0Model(pim);
            lr0.BuildModel();

            string dot = lr0.ToString();

            TransitionTable tt = TransitionTable.Create(lr0, info);
            var errdef = new SyntaxErrors() { TokenUnexpectedId = 1, TokenMissingId = 2, OtherErrorId = 3 };
            ParserEngine driver = new ParserEngine(tt, errdef);

            string source1 = "abc >> 123";
            var sr1 = new SourceReader(new StringReader(source1));

            Scanner scanner = new Scanner(info);
            scanner.SetTriviaTokens(WHITESPACE.Index);
            scanner.SetSource(sr1);

            Lexeme r;
            do
            {
                r = scanner.Read();

                driver.Input(r);
            } while (!r.IsEndOfStream);

            Assert.AreEqual(1, driver.AcceptedCount);
            Assert.AreEqual("A", driver.GetResult(0, el));
            Assert.AreEqual(0, el.Count);

            ParserEngine driver2 = new ParserEngine(tt, errdef);

            string source2 = "abc > > 123";
            var sr2 = new SourceReader(new StringReader(source2));

            scanner.SetSource(sr2);
            do
            {
                r = scanner.Read();

                driver2.Input(r);
            } while (!r.IsEndOfStream);

            var el2 = errorManager.CreateErrorList();

            Assert.AreEqual(1, driver2.AcceptedCount);
            Assert.AreEqual("B", driver2.GetResult(0, el2));
            Assert.AreEqual(0, el2.Count);
        }

        class Node
        {
            public Node LeftChild { get; private set; }
            public Node RightChild { get; private set; }
            public string Label { get; private set; }

            public Node(string label, Node left, Node right)
            {
                Label = label;
                LeftChild = left;
                RightChild = right;
            }

            public override string ToString()
            {
                return String.Format("{0}({1},{2})", Label,
                    LeftChild != null ? LeftChild.ToString() : null,
                    RightChild != null ? RightChild.ToString() : null);
            }
        }


        [Test]
        public void ParserErrorRecoveryTest()
        {
            Lexicon binaryTreeSyntax = new Lexicon();
            var lex = binaryTreeSyntax.Lexer;

            //lex
            Token LEFTPH = lex.DefineToken(RE.Symbol('('));
            Token RIGHTPH = lex.DefineToken(RE.Symbol(')'));
            Token COMMA = lex.DefineToken(RE.Symbol(','));
            Token LETTER = lex.DefineToken(RE.Range('a', 'z') | RE.Range('A', 'Z'), "ID");

            //grammar
            Production<Node> NodeParser = new Production<Node>();
            NodeParser.Rule =
                (from a in LETTER
                 from _1 in LEFTPH
                 from left in NodeParser
                 from _2 in COMMA
                 from right in NodeParser
                 from _3 in RIGHTPH
                 select new Node(a.Value.Content, left, right))
                | Grammar.Empty<Node>(null);

            var builder = new ForkableScannerBuilder(binaryTreeSyntax.CreateScannerInfo());

            const string correct = "A(B(,),C(,))";

            string source = "A((B(,),C(,)";
            SourceReader sr = new SourceReader(
                new StringReader(source));

            var info = binaryTreeSyntax.CreateScannerInfo();
            Scanner scanner = new Scanner(info);
            scanner.SetSource(sr);

            CompilationErrorManager errorManager = new CompilationErrorManager();
            errorManager.DefineError(1, 0, CompilationStage.Parsing, "Unexpected token '{0}'");
            errorManager.DefineError(2, 0, CompilationStage.Parsing, "Missing token '{0}'");
            errorManager.DefineError(3, 0, CompilationStage.Parsing, "Invalid token found, did you mean '{0}' ?");
            errorManager.DefineError(4, 0, CompilationStage.Parsing, "Syntax error");

            ProductionInfoManager pim = new ProductionInfoManager(NodeParser.SuffixedBy(Grammar.Eos()));

            LR0Model lr0 = new LR0Model(pim);
            lr0.BuildModel();

            string dot = lr0.ToString();

            TransitionTable tt = TransitionTable.Create(lr0, info);

            SyntaxErrors errDef = new SyntaxErrors() { TokenUnexpectedId = 1, TokenMissingId = 2, OtherErrorId = 4, TokenMistakeId = 3 };

            ParserEngine driver = new ParserEngine(tt, errDef);

            Lexeme r;
            do
            {
                r = scanner.Read();

                driver.Input(r);
            } while (!r.IsEndOfStream);

            var result = driver.GetResult(0, errorManager.CreateErrorList());

            ;
        }

        class TreeParser : ParserBase<Node>
        {
            Token LEFTPH;
            Token RIGHTPH;
            Token COMMA;
            Token LETTER;

            public TreeParser(CompilationErrorManager em) : base(em) { }

            protected override void OnDefineLexer(Lexicon lexicon, ICollection<Token> triviaTokens)
            {
                var lex = lexicon.Lexer;

                //lex
                LEFTPH = lex.DefineToken(RE.Symbol('('));
                RIGHTPH = lex.DefineToken(RE.Symbol(')'));
                COMMA = lex.DefineToken(RE.Symbol(','));
                LETTER = lex.DefineToken(RE.Range('a', 'z') | RE.Range('A', 'Z'), "ID");
            }

            protected override ProductionBase<Node> OnDefineGrammar()
            {
                //grammar
                Production<Node> NodeParser = new Production<Node>();
                NodeParser.Rule =
                    (from a in LETTER
                     from _1 in LEFTPH
                     from left in NodeParser
                     from _2 in COMMA
                     from right in NodeParser
                     from _3 in RIGHTPH
                     select new Node(a.Value.Content, left, right))
                    | Grammar.Empty<Node>(null);

                return NodeParser;
            }
        }

        [Test]
        public void ParserBaseTest()
        {
            string source = "A((B(,),C(,)";
            CompilationErrorManager em = new CompilationErrorManager();

            var parser = new TreeParser(em);

            var result = parser.Parse(source, em.CreateErrorList());
            ;
        }

        class GetDef
        {
            public IEnumerable<string> Statements { get; set; }
        }

        class PropDef
        {
            public string PropName { get; set; }
            public GetDef GetDef { get; set; }
        }

        [Test]
        public void MultipleLexerParsingTest()
        {
            Lexicon lexicon = new Lexicon();
            Lexer global = lexicon.Lexer;
            Lexer keywords = global.CreateSubLexer();

            var PROPERTY = global.DefineToken(RE.Literal("property"));
            var ID = global.DefineToken(RE.Range('a', 'z').Concat(
                (RE.Range('a', 'z') | RE.Range('0', '9')).Many()), "ID");
            var NUM = global.DefineToken(RE.Range('0', '9').Many1(), "NUM");
            var EQ = global.DefineToken(RE.Symbol('='));
            var SEMICOLON = global.DefineToken(RE.Symbol(';'));
            var LB = global.DefineToken(RE.Symbol('{'));
            var RB = global.DefineToken(RE.Symbol('}'));
            var WHITESPACE = global.DefineToken(RE.Symbol(' ').Union(RE.Symbol('\t')), "[ ]");

            var GET = keywords.DefineToken(RE.Literal("get"));

            var assignStatement =
                from id in ID
                from eq in EQ
                from value in NUM
                from st in SEMICOLON
                select id.Value + "=" + value.Value;

            var getDef =
                from _get in GET
                from lb in LB
                from statements in assignStatement.Many()
                from rb in RB
                select new GetDef{ Statements = statements };

            var propDef =
                from _prop in PROPERTY
                from id in ID
                from lb in LB
                from getdef in getDef
                from rb in RB
                select new PropDef{ PropName = id.Value.Content, GetDef = getdef };

            string source = "property get { get { get = 1; } }";
            SourceReader sr = new SourceReader(
                new StringReader(source));

            var info = lexicon.CreateScannerInfo();
            Scanner scanner = new Scanner(info);
            scanner.SetTriviaTokens(WHITESPACE.Index);
            scanner.SetSource(sr);

            CompilationErrorManager errorManager = new CompilationErrorManager();
            errorManager.DefineError(1, 0, CompilationStage.Parsing, "Unexpected token '{0}'");
            errorManager.DefineError(2, 0, CompilationStage.Parsing, "Missing token '{0}'");
            errorManager.DefineError(3, 0, CompilationStage.Parsing, "Syntax error");

            ProductionInfoManager pim = new ProductionInfoManager(propDef.SuffixedBy(Grammar.Eos()));

            LR0Model lr0 = new LR0Model(pim);
            lr0.BuildModel();

            string dot = lr0.ToString();

            TransitionTable tt = TransitionTable.Create(lr0, info);

            SyntaxErrors errDef = new SyntaxErrors() { TokenUnexpectedId = 1, TokenMissingId = 2, OtherErrorId = 3 };

            ParserEngine driver = new ParserEngine(tt, errDef);

            Lexeme r;
            do
            {
                r = scanner.Read();

                driver.Input(r);
            } while (!r.IsEndOfStream);

            var el = errorManager.CreateErrorList();

            var result = (PropDef)driver.GetResult(0, el);

            Assert.AreEqual(0, el.Count);
            Assert.AreEqual("get", result.PropName);
            Assert.AreEqual("get=1", result.GetDef.Statements.First());
        }

        class OperatorManyParser : ParserBase<IEnumerable<string>>
        {
            Token LEFTPH;
            Token RIGHTPH;
            Token COMMA;
            Token LETTER;

            public OperatorManyParser(CompilationErrorManager em) : base(em) { }

            protected override void OnDefineLexer(Lexicon lexicon, ICollection<Token> triviaTokens)
            {
                var lex = lexicon.Lexer;

                //lex
                LEFTPH = lex.DefineToken(RE.Symbol('('));
                RIGHTPH = lex.DefineToken(RE.Symbol(')'));
                COMMA = lex.DefineToken(RE.Symbol(','));
                LETTER = lex.DefineToken(RE.Range('a', 'z') | RE.Range('A', 'Z'), "ID");
            }

            protected override ProductionBase<IEnumerable<string>> OnDefineGrammar()
            {
                //grammar
                Production<IEnumerable<string>> P = new Production<IEnumerable<string>>();
                P.Rule =                    
                    from list in LETTER.Many(COMMA)
                    select list.Select(z => z.Value.Content).ToArray() as IEnumerable<string>;

                return P;
            }
        }

        [Test]
        public void OperatorManyTest()
        {
            string source = "";
            CompilationErrorManager em = new CompilationErrorManager();

            var parser = new OperatorManyParser(em);

            var result = parser.Parse(source, em.CreateErrorList());
            ;
        }
    }
}
