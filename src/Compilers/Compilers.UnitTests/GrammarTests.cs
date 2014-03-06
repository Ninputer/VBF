using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VBF.Compilers;
using VBF.Compilers.Parsers;
using VBF.Compilers.Scanners;
using RE = VBF.Compilers.Scanners.RegularExpression;

namespace Compilers.UnitTests
{
    [TestFixture]
    public class GrammarTests
    {
        class StatementNode
        {

        }

        class VarDeclStatementNode : StatementNode
        {
            private Lexeme typename;
            private Lexeme genericTypename;
            private Lexeme varname;

            public VarDeclStatementNode(Lexeme typename, Lexeme genericTypename, Lexeme varname)
            {
                // TODO: Complete member initialization
                this.typename = typename;
                this.genericTypename = genericTypename;
                this.varname = varname;
            }

        }

        class ExpressionStatementNode : StatementNode
        {
            private ExpressionNode expression;

            public ExpressionStatementNode(ExpressionNode expression)
            {
                // TODO: Complete member initialization
                this.expression = expression;
            }

        }

        class AmbiguityStatementNode : StatementNode
        {
            public StatementNode s1;
            public StatementNode s2;

            public AmbiguityStatementNode(StatementNode s1, StatementNode s2)
            {
                // TODO: Complete member initialization
                this.s1 = s1;
                this.s2 = s2;
            }

        }

        class ExpressionNode
        {

        }

        class BinaryExpressionNode : ExpressionNode
        {
            private Lexeme op;
            private ExpressionNode left;
            private ExpressionNode right;

            public BinaryExpressionNode(Lexeme op, ExpressionNode left, ExpressionNode right)
            {
                // TODO: Complete member initialization
                this.op = op;
                this.left = left;
                this.right = right;
            }

        }

        class BasicExpressionNode : ExpressionNode
        {
            private Lexeme exp;

            public BasicExpressionNode(Lexeme exp)
            {
                // TODO: Complete member initialization
                this.exp = exp;
            }

        }

        class AmbiguityParser : ParserBase<IEnumerable<StatementNode>>
        {
            public AmbiguityParser(CompilationErrorManager em) : base(em) { }

            Token ID;
            Token NUM;
            Token GREATER;
            Token LESS;
            Token SEMICOLON;

            protected override void OnDefineLexer(Lexicon lexicon, ICollection<Token> triviaTokens)
            {
                ID = lexicon.Lexer.DefineToken(RE.Range('a', 'z').Concat(
                    (RE.Range('a', 'z') | RE.Range('0', '9')).Many()), "ID");
                NUM = lexicon.Lexer.DefineToken(RE.Range('0', '9').Many1(), "NUM");
                GREATER = lexicon.Lexer.DefineToken(RE.Symbol('>'));
                LESS = lexicon.Lexer.DefineToken(RE.Symbol('<'));
                SEMICOLON = lexicon.Lexer.DefineToken(RE.Symbol(';'));

                var WHITESPACE = lexicon.Lexer.DefineToken(RE.Symbol(' ').Union(RE.Symbol('\t')), "white space");

                triviaTokens.Add(WHITESPACE);
            }

            protected override ProductionBase<IEnumerable<StatementNode>> OnDefineGrammar()
            {
                Production<IEnumerable<StatementNode>> Statements = new Production<IEnumerable<StatementNode>>();
                Production<StatementNode> Statement = new Production<StatementNode>();
                Production<StatementNode> VarDeclStatement = new Production<StatementNode>();
                Production<StatementNode> ExpressionStatement = new Production<StatementNode>();
                Production<ExpressionNode> Expression = new Production<ExpressionNode>();
                Production<ExpressionNode> Comparison = new Production<ExpressionNode>();

                Statements.Rule = Statement.Many();
                Statement.Rule = VarDeclStatement | ExpressionStatement;

                Statement.AmbiguityAggregator = (s1, s2) => new AmbiguityStatementNode(s1, s2);

                VarDeclStatement.Rule =
                    from typename in ID
                    from _less in LESS
                    from genericTypename in ID
                    from _greater in GREATER
                    from varname in ID
                    from _st in SEMICOLON
                    select new VarDeclStatementNode(typename, genericTypename, varname) as StatementNode;

                ExpressionStatement.Rule =
                    from expression in Expression
                    from _st in SEMICOLON
                    select new ExpressionStatementNode(expression) as StatementNode;

                var BasicExpression =
                    from exp in ID.AsTerminal() | NUM.AsTerminal()
                    select new BasicExpressionNode(exp) as ExpressionNode;

                Expression.Rule = BasicExpression | Comparison;

                Comparison.Rule =
                    BasicExpression |
                    from left in Comparison
                    from op in LESS.AsTerminal() | GREATER.AsTerminal()
                    from right in BasicExpression
                    select new BinaryExpressionNode(op, left, right) as ExpressionNode;

                return Statements;
            }
        }

        [Test]
        public void AmbiguityGrammarAggregationTest()
        {
            CompilationErrorManager em = new CompilationErrorManager();

            AmbiguityParser parser = new AmbiguityParser(em);

            const string input = "a<b>c;a<b>c;a<b>c;a<b>c;a<b>c;";

            var result = parser.Parse(input, em.CreateErrorList());
            var resarr = result.ToArray();

            Assert.AreEqual(5, resarr.Length);

            foreach (var r in resarr)
            {
                var ambStat = r as AmbiguityStatementNode;

                Assert.IsNotNull(ambStat);

                VarDeclStatementNode vardecl = ambStat.s1 as VarDeclStatementNode;
                if (vardecl == null)
                {
                    vardecl = ambStat.s2 as VarDeclStatementNode;
                }

                Assert.IsNotNull(vardecl);

                ExpressionStatementNode expstat = ambStat.s1 as ExpressionStatementNode;
                if (expstat == null)
                {
                    expstat = ambStat.s2 as ExpressionStatementNode;
                }

                Assert.IsNotNull(expstat);

            }
        }

        [Test]
        public void AmbiguityGrammarErrorTest()
        {
            CompilationErrorManager em = new CompilationErrorManager();

            AmbiguityParser parser = new AmbiguityParser(em);

            const string input = "a<b>>c;";

            var el = em.CreateErrorList();
            var result = parser.Parse(input, el);

            var r = result.First() as AmbiguityStatementNode;

            Assert.IsNotNull(r);
            Assert.AreEqual(1, el.Count);

        }
    }
}
