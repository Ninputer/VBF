using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;
using VBF.Compilers.Scanners;
using VBF.Compilers.Parsers.Combinators;
using RE = VBF.Compilers.Scanners.RegularExpression;
using System.Globalization;
using VBF.MiniSharp.Ast;
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
        private Token K_SYSTEM;
        private Token K_CONSOLE;
        private Token K_WRITELINE;
        private Token K_LENGTH;
        private Token K_TRUE;
        private Token K_FALSE;
        private Token K_THIS;
        private Token K_NEW;

        //id and literals
        private Token ID; //identifier
        private Token INTEGER_LITERAL; //integer literal

        //symbols
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

        //skips
        private Token WHITESPACE;
        private Token COMMENT;
        private Token LINE_BREAKER;

        //grammar
        private ParserReference<Program> PProgram = new ParserReference<Program>();
        private ParserReference<MainClass> PMainClass = new ParserReference<MainClass>();
        private ParserReference<ClassDecl> PClassDecl = new ParserReference<ClassDecl>();
        private ParserReference<VarDecl> PVarDecl = new ParserReference<VarDecl>();
        private ParserReference<MethodDecl> PMethodDecl = new ParserReference<MethodDecl>();
        private ParserReference<Formal[]> PFormalList = new ParserReference<Formal[]>();
        private ParserReference<Formal> PFormalRest = new ParserReference<Formal>();
        private ParserReference<Ast.Type> PType = new ParserReference<Ast.Type>();
        private ParserReference<Ast.Type> PIntArrayType = new ParserReference<Ast.Type>();
        private ParserReference<Ast.Type> PBoolType = new ParserReference<Ast.Type>();
        private ParserReference<Ast.Type> PIntType = new ParserReference<Ast.Type>();
        private ParserReference<Ast.Type> PIdType = new ParserReference<Ast.Type>();
        private ParserReference<Statement> PStatement = new ParserReference<Statement>();
        private ParserReference<Statement> PIfElse = new ParserReference<Statement>();
        private ParserReference<Statement> PWhile = new ParserReference<Statement>();
        private ParserReference<Statement> PWriteLine = new ParserReference<Statement>();
        private ParserReference<Statement> PAssignment = new ParserReference<Statement>();
        private ParserReference<Statement> PArrayAssignment = new ParserReference<Statement>();
        private ParserReference<Expression> PExp = new ParserReference<Expression>();
        private ParserReference<Expression> PFactor = new ParserReference<Expression>();
        private ParserReference<Expression> PTerm = new ParserReference<Expression>();
        private ParserReference<Expression> PComparand = new ParserReference<Expression>();
        private ParserReference<Expression> PComparison = new ParserReference<Expression>();
        private ParserReference<Expression> PAnd = new ParserReference<Expression>();
        private ParserReference<Expression> POr = new ParserReference<Expression>();
        private ParserReference<Expression> PNot = new ParserReference<Expression>();
        private ParserReference<Expression> PVariable = new ParserReference<Expression>();
        private ParserReference<Expression> PThis = new ParserReference<Expression>();
        private ParserReference<Expression> PArrayIndex = new ParserReference<Expression>();
        private ParserReference<Expression> PArrayLength = new ParserReference<Expression>();
        private ParserReference<Expression> PCall = new ParserReference<Expression>();
        private ParserReference<Expression> PNumberLiteral = new ParserReference<Expression>();
        private ParserReference<Expression> PBoolLiteral = new ParserReference<Expression>();
        private ParserReference<Expression> PNew = new ParserReference<Expression>();
        private ParserReference<Expression[]> PExpList = new ParserReference<Expression[]>();
        private ParserReference<Expression> PExpRest = new ParserReference<Expression>();

        protected override void OnDefineLexer(Compilers.Scanners.Lexicon lexicon, ICollection<int> skippedTokens)
        {
            var lex = lexicon.DefaultLexer;

            //keywords
            K_CLASS = lex.DefineToken(RE.Literal("class"));
            K_PUBLIC = lex.DefineToken(RE.Literal("public"));
            K_STATIC = lex.DefineToken(RE.Literal("static"));
            K_VOID = lex.DefineToken(RE.Literal("void"));
            K_MAIN = lex.DefineToken(RE.Literal("Main"));
            K_STRING = lex.DefineToken(RE.Literal("string"));
            K_RETURN = lex.DefineToken(RE.Literal("return"));
            K_INT = lex.DefineToken(RE.Literal("int"));
            K_BOOL = lex.DefineToken(RE.Literal("bool"));
            K_IF = lex.DefineToken(RE.Literal("if"));
            K_ELSE = lex.DefineToken(RE.Literal("else"));
            K_WHILE = lex.DefineToken(RE.Literal("while"));
            K_SYSTEM = lex.DefineToken(RE.Literal("System"));
            K_CONSOLE = lex.DefineToken(RE.Literal("Console"));
            K_WRITELINE = lex.DefineToken(RE.Literal("WriteLine"));
            K_LENGTH = lex.DefineToken(RE.Literal("Length"));
            K_TRUE = lex.DefineToken(RE.Literal("true"));
            K_FALSE = lex.DefineToken(RE.Literal("false"));
            K_THIS = lex.DefineToken(RE.Literal("this"));
            K_NEW = lex.DefineToken(RE.Literal("new"));

            //id & literals

            var lettersCategories = new[] 
            { 
                UnicodeCategory.LetterNumber,
                UnicodeCategory.LowercaseLetter,
                UnicodeCategory.ModifierLetter,
                UnicodeCategory.OtherLetter,
                UnicodeCategory.TitlecaseLetter,
                UnicodeCategory.UppercaseLetter
            };

            var RE_IdChar = RE.CharsOf(c => lettersCategories.Contains(Char.GetUnicodeCategory(c))) | RE.Symbol('_');

            ID = lex.DefineToken(RE_IdChar >>
                (RE_IdChar | RE.Range('0', '9')).Many());
            INTEGER_LITERAL = lex.DefineToken(RE.Range('0', '9').Many1());

            //symbols

            LOGICAL_AND = lex.DefineToken(RE.Literal("&&"));
            LOGICAL_OR = lex.DefineToken(RE.Literal("||"));
            LOGICAL_NOT = lex.DefineToken(RE.Symbol('!'));
            LESS = lex.DefineToken(RE.Symbol('<'));
            GREATER = lex.DefineToken(RE.Symbol('>'));
            EQUAL = lex.DefineToken(RE.Literal("=="));
            ASSIGN = lex.DefineToken(RE.Symbol('='));
            PLUS = lex.DefineToken(RE.Symbol('+'));
            MINUS = lex.DefineToken(RE.Symbol('-'));
            ASTERISK = lex.DefineToken(RE.Symbol('*'));
            SLASH = lex.DefineToken(RE.Symbol('/'));
            LEFT_PH = lex.DefineToken(RE.Symbol('('));
            RIGHT_PH = lex.DefineToken(RE.Symbol(')'));
            LEFT_BK = lex.DefineToken(RE.Symbol('['));
            RIGHT_BK = lex.DefineToken(RE.Symbol(']'));
            LEFT_BR = lex.DefineToken(RE.Symbol('{'));
            RIGHT_BR = lex.DefineToken(RE.Symbol('}'));
            COMMA = lex.DefineToken(RE.Symbol(','));
            COLON = lex.DefineToken(RE.Symbol(':'));
            SEMICOLON = lex.DefineToken(RE.Symbol(';'));
            DOT = lex.DefineToken(RE.Symbol('.'));

            //skips

            var RE_SpaceChar = RE.CharsOf(c => Char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator);

            WHITESPACE = lex.DefineToken(RE_SpaceChar | RE.CharSet("\u0009\u000B\u000C"));

            LINE_BREAKER = lex.DefineToken(
                RE.CharSet("\u000D\u000A\u0085\u2028\u2029") |
                RE.Literal("\r\n")
            );

            var RE_InputChar = RE.CharsOf(c => !"\u000D\u000A\u0085\u2028\u2029".Contains(c));
            var RE_NotSlashOrAsterisk = RE.CharsOf(c => !"/*".Contains(c));
            var RE_DelimitedCommentSection = RE.Symbol('/') | (RE.Symbol('*').Many() >> RE_NotSlashOrAsterisk);

            COMMENT = lex.DefineToken(
                (RE.Literal("//") >> RE_InputChar.Many()) |
                (RE.Literal("/*") >> RE_DelimitedCommentSection.Many() >> RE.Symbol('*').Many1() >> RE.Symbol('/'))
            );

            skippedTokens.Add(WHITESPACE.Index);
            skippedTokens.Add(LINE_BREAKER.Index);
            skippedTokens.Add(COMMENT.Index);
        }

        protected override Compilers.Parsers.Combinators.Parser<Ast.Program> OnDefineParser()
        {
            PProgram.Reference = // MainClass ClassDecl*
                from main in PMainClass
                from classes in PClassDecl.Many()
                select new Program();

            PMainClass.Reference = // class id { public static void Main(string[] id) { statement }}
                from _c in K_CLASS
                from className in ID
                from _1 in LEFT_BR
                from _p in K_PUBLIC
                from _s in K_STATIC
                from _v in K_VOID
                from _m in K_MAIN
                from _2 in LEFT_PH
                from _str in K_STRING
                from _3 in LEFT_BK
                from _4 in RIGHT_BK
                from _i in ID
                from _5 in RIGHT_PH
                from _6 in LEFT_BR
                /*from statement in PStatement*/
                from _7 in RIGHT_BR
                from _8 in RIGHT_BR
                select new MainClass();

            var classMembers =
                from _1 in LEFT_BR
                from varDecls in PVarDecl.Many()
                from methodDecls in PMethodDecl.Many()
                from _2 in RIGHT_BR
                select new { Variables = varDecls, Methods = methodDecls };

            var classDeclSimple = // { VarDecl* MethodDecl* }
                from members in classMembers
                select new { BaseClassName = default(string), Members = members };

            var classDeclInherits = //: id { VarDecl* MethodDecl* }
                from _1 in COLON
                from baseClassName in ID
                from members in classMembers
                select new { BaseClassName = baseClassName.Value, Members = members };

            PClassDecl.Reference = //class id
                from _c in K_CLASS
                from className in ID
                from def in (classDeclSimple | classDeclInherits)
                select default(ClassDecl);

            PVarDecl.Reference = // Type id;
                from type in PType
                from varName in ID
                from _sc in SEMICOLON
                select new VarDecl();

            PMethodDecl.Reference = // public Type id (FormalList) { VarDecl* Statement* return Exp; }
                from _p in K_PUBLIC
                from type in PType
                from methodName in ID
                from _1 in LEFT_PH
                from formals in PFormalList
                from _2 in RIGHT_PH
                from _3 in LEFT_BR
                from varDecls in PVarDecl.Many()
                from statements in PStatement.Many()
                from _r in K_RETURN
                from returnExp in PExp
                from _sc in SEMICOLON
                from _4 in RIGHT_BR
                select new MethodDecl();

            var paramFormal = 
                from paramType in PType
                from paramName in PIdType
                select new Formal();

            PFormalList.Reference = // Type id FormalRest* | <empty>
                Parsers.Succeed(new Formal[0]) |
                from first in paramFormal
                from rest in PFormalRest.Many()
                select new[] { first }.Concat(rest).ToArray();

            PFormalRest.Reference = // , Type id
                paramFormal.PrefixedBy(COMMA.AsParser());


            PType.Reference = // int[] | bool | int | id 
                PIntArrayType | PBoolType | PIntType | PIdType;

            PIntArrayType.Reference = //int[]
                from _int in K_INT
                from _lb in LEFT_BK
                from _rb in RIGHT_BK
                select default(Ast.Type);

            PBoolType.Reference = // bool
                from _bool in K_BOOL
                select default(Ast.Type);

            PIntType.Reference = // int
                from _int in K_INT
                select default(Ast.Type);

            PIdType.Reference = // id
                from type in ID
                select default(Ast.Type);

            PStatement.Reference = // { statement*} | ifelse | while | writeline | assign | array assign
                (from _1 in LEFT_BR from stmts in PStatement.Many() from _2 in RIGHT_BR select default(Statement)) |
                PIfElse |
                PWhile |
                PWriteLine |
                PAssignment |
                PArrayAssignment;

            return PProgram;
        }
    }
}
