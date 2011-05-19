using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;
using VBF.Compilers.Scanners;
using VBF.Compilers.Parsers.Combinators;
using RE = VBF.Compilers.Scanners.RegularExpression;
using System.Globalization;
namespace VBF.MiniSharp
{
    public class MiniSharpParser : ParserFrame<Ast.Program>
    {
        public MiniSharpParser() : base(new CompilationErrorManager(), 201, 202) { }

        //keywords
        private Token K_CLASS;
        private Token K_PUBLIC;
        private Token K_STATIC;
        private Token K_VOID;
        private Token K_MAIN;
        private Token K_STRING;
        private Token K_RETURN;
        private Token K_INT;
        private Token K_BOOL;
        private Token K_IF;
        private Token K_ELSE;
        private Token K_WHILE;
        private Token K_WRITELINE;
        private Token K_LENGTH;
        private Token K_TRUE;
        private Token K_FALSE;
        private Token K_THIS;
        private Token K_NEW;


        private Token ID; //identifier
        private Token INTEGER_LITERAL; //integer literal
        private Token LOGICAL_AND; // &&
        private Token LOGICAL_OR; // ||
        private Token LOGICAL_NOT; // !
        private Token LESS; // <
        private Token GREATER; // >
        private Token EQUAL; // ==
        private Token ASSIGN; // =
        private Token PLUS; // +
        private Token MINUS; // -
        private Token ASTERISK; // *
        private Token SLASH; // /
        private Token LEFT_PH; // (
        private Token RIGHT_PH; // )
        private Token LEFT_BK; // [
        private Token RIGHT_BK; // ]
        private Token LEFT_BR; // {
        private Token RIGHT_BR; // }
        private Token COMMA; // ,
        private Token COLON; // :
        private Token SEMICOLON; // ;
        private Token DOT; // .

        private Token WHITESPACE;
        private Token COMMENT;
        private Token LINE_BREAKER;

        protected override void OnDefineLexer(Compilers.Scanners.Lexicon lexicon, ICollection<int> skippedTokens)
        {
            var lex = lexicon.DefaultLexer;

            var lettersCategories = new[] 
            { 
                UnicodeCategory.LetterNumber,
                UnicodeCategory.LowercaseLetter,
                UnicodeCategory.ModifierLetter,
                UnicodeCategory.OtherLetter,
                UnicodeCategory.TitlecaseLetter,
                UnicodeCategory.UppercaseLetter
            };

            var RE_IDCHAR = RE.CharsOf(c => lettersCategories.Contains(Char.GetUnicodeCategory(c))) | RE.Symbol('_');

            //keywords
            K_CLASS = lex.DefineToken(RE.Literal("class"));
            K_PUBLIC = lex.DefineToken(RE.Literal("public"));
            K_STATIC = lex.DefineToken(RE.Literal("static"));
            K_VOID = lex.DefineToken(RE.Literal("void"));
            K_MAIN = lex.DefineToken(RE.Literal("main"));
            K_STRING = lex.DefineToken(RE.Literal("string"));
            K_RETURN = lex.DefineToken(RE.Literal("return"));
            K_INT = lex.DefineToken(RE.Literal("int"));
            K_BOOL = lex.DefineToken(RE.Literal("bool"));
            K_IF = lex.DefineToken(RE.Literal("if"));
            K_ELSE = lex.DefineToken(RE.Literal("else"));
            K_WHILE = lex.DefineToken(RE.Literal("while"));
            K_WRITELINE = lex.DefineToken(RE.Literal("System.Console.WriteLine"));
            K_LENGTH = lex.DefineToken(RE.Literal("Length"));
            K_TRUE = lex.DefineToken(RE.Literal("true"));
            K_FALSE = lex.DefineToken(RE.Literal("false"));
            K_THIS = lex.DefineToken(RE.Literal("this"));
            K_NEW = lex.DefineToken(RE.Literal("new"));

            ID = lex.DefineToken(RE_IDCHAR.Concat(
                (RE_IDCHAR | RE.Range('0', '9')).Many()));
            INTEGER_LITERAL = lex.DefineToken(RE.Range('0', '9').Many1());
            WHITESPACE = lex.DefineToken(RE.Symbol(' ').Many());
        }

        protected override Compilers.Parsers.Combinators.Parser<Ast.Program> OnDefineParser()
        {
            throw new NotImplementedException();
        }
    }
}
