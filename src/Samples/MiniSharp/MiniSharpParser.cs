using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;
using VBF.Compilers.Scanners;
using VBF.Compilers.Parsers;
using RE = VBF.Compilers.Scanners.RegularExpression;
using System.Globalization;
using VBF.MiniSharp.Ast;
namespace VBF.MiniSharp
{
    public class MiniSharpParser : ParserBase<Ast.Program>
    {
        public MiniSharpParser(CompilationErrorManager errorManager) : base(errorManager) { }

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
        private Production<Program> PProgram = new Production<Program>();
        private Production<MainClass> PMainClass = new Production<MainClass>();
        private Production<ClassDecl> PClassDecl = new Production<ClassDecl>();
        private Production<FieldDecl> PFieldDecl = new Production<FieldDecl>();
        private Production<MethodDecl> PMethodDecl = new Production<MethodDecl>();
        private Production<Formal[]> PFormalList = new Production<Formal[]>();
        private Production<Ast.Type> PType = new Production<Ast.Type>();
        private Production<Ast.Type> PIntArrayType = new Production<Ast.Type>();
        private Production<Ast.Type> PBoolType = new Production<Ast.Type>();
        private Production<Ast.Type> PIntType = new Production<Ast.Type>();
        private Production<Ast.Type> PIdType = new Production<Ast.Type>();
        private Production<Statement> PStatement = new Production<Statement>();
        private Production<Statement> PIfElse = new Production<Statement>();
        private Production<Statement> PWhile = new Production<Statement>();
        private Production<Statement> PWriteLine = new Production<Statement>();
        private Production<Statement> PAssignment = new Production<Statement>();
        private Production<Statement> PArrayAssignment = new Production<Statement>();
        private Production<Statement> PVarDeclStmt = new Production<Statement>();
        private Production<Expression> PExp = new Production<Expression>();
        private Production<Expression> PFactor = new Production<Expression>();
        private Production<Expression> PTerm = new Production<Expression>();
        private Production<Expression> PComparand = new Production<Expression>();
        private Production<Expression> PComparison = new Production<Expression>();
        private Production<Expression> PAnd = new Production<Expression>();
        private Production<Expression> POr = new Production<Expression>();
        private Production<Expression> PNot = new Production<Expression>();
        private Production<Expression> PVariable = new Production<Expression>();
        private Production<Expression> PThis = new Production<Expression>();
        private Production<Expression> PArrayLookup = new Production<Expression>();
        private Production<Expression> PArrayLength = new Production<Expression>();
        private Production<Expression> PCall = new Production<Expression>();
        private Production<Expression> PNumberLiteral = new Production<Expression>();
        private Production<Expression> PBoolLiteral = new Production<Expression>();
        private Production<Expression> PNewObj = new Production<Expression>();
        private Production<Expression> PNewArray = new Production<Expression>();
        private Production<Expression[]> PExpList = new Production<Expression[]>();

        protected override void OnDefineLexer(Compilers.Scanners.Lexicon lexicon, ICollection<Token> skippedTokens)
        {
            var lettersCategories = new HashSet<UnicodeCategory>()
            { 
                UnicodeCategory.LetterNumber,
                UnicodeCategory.LowercaseLetter,
                UnicodeCategory.ModifierLetter,
                UnicodeCategory.OtherLetter,
                UnicodeCategory.TitlecaseLetter,
                UnicodeCategory.UppercaseLetter
            };

            RE RE_IdChar = null;
            RE RE_SpaceChar = null;
            RE RE_InputChar = null;
            RE RE_NotSlashOrAsterisk = null;

            CharSetExpressionBuilder charSetBuilder = new CharSetExpressionBuilder();

            charSetBuilder.DefineCharSet(c => lettersCategories.Contains(Char.GetUnicodeCategory(c)), re => RE_IdChar = re | RE.Symbol('_'));
            charSetBuilder.DefineCharSet(c => Char.GetUnicodeCategory(c) == UnicodeCategory.SpaceSeparator, re => RE_SpaceChar = re);
            charSetBuilder.DefineCharSet(c => "\u000D\u000A\u0085\u2028\u2029".IndexOf(c) < 0, re => RE_InputChar = re);
            charSetBuilder.DefineCharSet(c => "/*".IndexOf(c) < 0, re => RE_NotSlashOrAsterisk = re);

            charSetBuilder.Build();

            var lex = lexicon.Lexer;

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

            ID = lex.DefineToken(RE_IdChar >>
                (RE_IdChar | RE.Range('0', '9')).Many(), "identifier");
            INTEGER_LITERAL = lex.DefineToken(RE.Range('0', '9').Many1(), "integer literal");

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

            WHITESPACE = lex.DefineToken(RE_SpaceChar | RE.CharSet("\u0009\u000B\u000C"));

            LINE_BREAKER = lex.DefineToken(
                RE.CharSet("\u000D\u000A\u0085\u2028\u2029") |
                RE.Literal("\r\n")
            );


            var RE_DelimitedCommentSection = RE.Symbol('/') | (RE.Symbol('*').Many() >> RE_NotSlashOrAsterisk);

            COMMENT = lex.DefineToken(
                (RE.Literal("//") >> RE_InputChar.Many()) |
                (RE.Literal("/*") >> RE_DelimitedCommentSection.Many() >> RE.Symbol('*').Many1() >> RE.Symbol('/')),
                "comment");

            skippedTokens.Add(WHITESPACE);
            skippedTokens.Add(LINE_BREAKER);
            skippedTokens.Add(COMMENT);
        }

        protected override ProductionBase<Program> OnDefineGrammar()
        {
            PProgram.Rule = // MainClass ClassDecl*
                from main in PMainClass
                from classes in PClassDecl.Many()
                select new Program(main, classes.ToArray());

            PMainClass.Rule = // static class id { public static void Main(string[] id) { statement }}
                from _static1 in K_STATIC
                from _class in K_CLASS
                from className in ID
                from _1 in LEFT_BR
                from _public in K_PUBLIC
                from _static2 in K_STATIC
                from _void in K_VOID
                from _main in K_MAIN
                from _2 in LEFT_PH
                from _string in K_STRING
                from _3 in LEFT_BK
                from _4 in RIGHT_BK
                from arg in ID
                from _5 in RIGHT_PH
                from _6 in LEFT_BR
                from statements in PStatement.Many1()
                from _7 in RIGHT_BR
                from _8 in RIGHT_BR
                select new MainClass(className.Value, arg.Value, statements.ToArray());

            var classMembers =
                from _1 in LEFT_BR
                from varDecls in PFieldDecl.Many()
                from methodDecls in PMethodDecl.Many()
                from _2 in RIGHT_BR
                select new { Fields = varDecls.ToArray(), Methods = methodDecls.ToArray() };

            var classDeclSimple = // { VarDecl* MethodDecl* }
                from members in classMembers
                select new { BaseClassName = default(Lexeme), Members = members };

            var classDeclInherits = //: id { VarDecl* MethodDecl* }
                from _1 in COLON
                from baseClassName in ID
                from members in classMembers
                select new { BaseClassName = baseClassName, Members = members };

            PClassDecl.Rule = //class id
                from _class in K_CLASS
                from className in ID
                from def in (classDeclSimple | classDeclInherits)
                select new ClassDecl(className.Value, def.BaseClassName.GetValue(), def.Members.Fields, def.Members.Methods);

            PFieldDecl.Rule = // Type id;
                from type in PType
                from varName in ID
                from _sc in SEMICOLON
                select new FieldDecl(type, varName.Value);

            var methodBody =
                from _1 in LEFT_BR
                from statements in PStatement.Many()
                from _return in K_RETURN
                from returnExp in PExp
                from _sc in SEMICOLON
                from _2 in RIGHT_BR
                select new { Statements = statements.ToArray(), ReturnExp = returnExp };

            var methodNoBody =
                from _sc in SEMICOLON
                select new { Statements = default(Statement[]), ReturnExp = default(Expression) };

            PMethodDecl.Rule = // public Type id (FormalList) { Statement* return Exp; }
                from _public in K_PUBLIC
                from type in PType
                from methodName in ID
                from _1 in LEFT_PH
                from formals in PFormalList
                from _2 in RIGHT_PH
                from body in (methodBody | methodNoBody)
                select new MethodDecl(methodName.Value, type, formals, body.Statements, body.ReturnExp);

            var paramFormal =
                from paramType in PType
                from paramName in ID
                select new Formal(paramType, paramName.Value);

            PFormalList.Rule = // Type id FormalRest* | <empty>
                 from list in paramFormal.Many(COMMA)
                 select list.ToArray();

            PType.Rule = // int[] | bool | int | id 
                PIntArrayType | PBoolType | PIntType | PIdType;

            PIntArrayType.Rule = //int[]
                from _int in K_INT
                from _lb in LEFT_BK
                from _rb in RIGHT_BK
                select (Ast.Type)new IntArrayType();

            PBoolType.Rule = // bool
                from _bool in K_BOOL
                select (Ast.Type)new BooleanType();

            PIntType.Rule = // int
                from _int in K_INT
                select (Ast.Type)new IntegerType();

            PIdType.Rule = // id
                from type in ID
                select (Ast.Type)new IdentifierType(type.Value);

            //statements

            PStatement.Rule = // { statement*} | ifelse | while | writeline | assign | array assign | var decl
                (from _1 in LEFT_BR from stmts in PStatement.Many() from _2 in RIGHT_BR select (Statement)new Block(stmts != null ? stmts.ToArray() : null)) |
                PIfElse |
                PWhile |
                PWriteLine |
                PAssignment |
                PArrayAssignment |
                PVarDeclStmt;

            PIfElse.Rule = // if ( exp ) statement else statement
                from _if in K_IF
                from _1 in LEFT_PH
                from condExp in PExp
                from _2 in RIGHT_PH
                from truePart in PStatement
                from _else in K_ELSE
                from falsePart in PStatement
                select (Statement)new IfElse(condExp, truePart, falsePart, _if.Value.Span, _else.Value.Span);

            PWhile.Rule = // while ( exp ) statement
                from _while in K_WHILE
                from _1 in LEFT_PH
                from condExp in PExp
                from _2 in RIGHT_PH
                from loopBody in PStatement
                select (Statement)new While(condExp, loopBody, _while.Value.Span);

            PWriteLine.Rule = // System.Console.WriteLine( exp );
                from _sys in K_SYSTEM
                from _1 in DOT
                from _console in K_CONSOLE
                from _2 in DOT
                from _wl in K_WRITELINE
                from _3 in LEFT_PH
                from exp in PExp
                from _4 in RIGHT_PH
                from _sc in SEMICOLON
                select (Statement)new WriteLine(exp, new SourceSpan(_sys.Value.Span.StartLocation, _wl.Value.Span.EndLocation));

            PAssignment.Rule = // id = exp;
                from variable in ID
                from _eq in ASSIGN
                from value in PExp
                from _sc in SEMICOLON
                select (Statement)new Assign(variable.Value, value);

            PArrayAssignment.Rule = // id[ exp ] = exp ;
                from variable in ID
                from _1 in LEFT_BK
                from index in PExp
                from _2 in RIGHT_BK
                from _eq in ASSIGN
                from value in PExp
                from _sc in SEMICOLON
                select (Statement)new ArrayAssign(variable.Value, index, value);

            PVarDeclStmt.Rule = // Type id;
                from type in PType
                from varName in ID
                from _sc in SEMICOLON
                select (Statement)new VarDecl(type, varName.Value);

            //expressions

            //basic
            PNumberLiteral.Rule = // number
                from intvalue in INTEGER_LITERAL
                select (Expression)new IntegerLiteral(intvalue.Value);

            PBoolLiteral.Rule = // true | false
                from b in K_TRUE.AsTerminal() | K_FALSE.AsTerminal()
                select (Expression)new BooleanLiteral(b.Value);

            PThis.Rule = // this
                from _this in K_THIS
                select (Expression)new This(_this.Value.Span);

            PVariable.Rule = // id
                from varName in ID
                select (Expression)new Variable(varName.Value);

            PNewObj.Rule = // new id()
                from _new in K_NEW
                from typeName in ID
                from _1 in LEFT_PH
                from _2 in RIGHT_PH
                select (Expression)new NewObject(typeName.Value);

            PNewArray.Rule = // new int [exp]
                from _new in K_NEW
                from _int in K_INT
                from _1 in LEFT_BK
                from length in PExp
                from _2 in RIGHT_BR
                select (Expression)new NewArray(length, new SourceSpan(_1.Value.Span.EndLocation, _2.Value.Span.StartLocation));

            var foundationExp = // (exp) | number literal | true | false | this | id | new
                PNumberLiteral |
                PBoolLiteral |
                PThis |
                PVariable |
                PNewObj |
                PNewArray |
                PExp.PackedBy(LEFT_PH, RIGHT_PH);


            PCall.Rule = // exp.id(explist)
                from exp in foundationExp
                from _d in DOT
                from methodName in ID
                from _1 in LEFT_PH
                from args in PExpList
                from _2 in RIGHT_PH
                select (Expression)new Call(exp, methodName.Value, args);

            PArrayLookup.Rule = // exp[exp]
                from exp in foundationExp
                from _1 in LEFT_BK
                from index in PExp
                from _2 in RIGHT_BK
                select (Expression)new ArrayLookup(exp, index, new SourceSpan(_1.Value.Span.EndLocation, _2.Value.Span.StartLocation));

            PArrayLength.Rule = // exp.Length
                from exp in foundationExp
                from _d in DOT
                from _length in K_LENGTH
                select (Expression)new ArrayLength(exp, _length.Value.Span);

            var basicExp = foundationExp | PCall | PArrayLookup | PArrayLength;  //foundation >> call | id[exp] | id.Length

            //unary 

            PNot.Rule = // ! exp
                basicExp |
                from _n in LOGICAL_NOT
                from exp in PNot
                select (Expression)new Not(exp, _n.Value.Span);

            //binary

            PFactor.Rule = // exp | !exp
                PNot;

            PTerm.Rule = // term * factor | factor
                PFactor |
                from term in PTerm
                from op in (ASTERISK.AsTerminal() | SLASH.AsTerminal())
                from factor in PFactor
                select (Expression)new Binary(op.Value, term, factor);

            PComparand.Rule = // comparand + term | term
                PTerm |
                from comparand in PComparand
                from op in (PLUS.AsTerminal() | MINUS.AsTerminal())
                from term in PTerm
                select (Expression)new Binary(op.Value, comparand, term);

            PComparison.Rule =// comparison < comparand | comparand
                PComparand |
                from comparison in PComparison
                from op in (LESS.AsTerminal() | GREATER.AsTerminal() | EQUAL.AsTerminal())
                from comparand in PComparand
                select (Expression)new Binary(op.Value, comparison, comparand);

            PAnd.Rule = // andexp && comparison | comparison
                PComparison |
                from andexp in PAnd
                from op in LOGICAL_AND
                from comparison in PComparison
                select (Expression)new Binary(op.Value, andexp, comparison);

            POr.Rule =
                PAnd |
                from orexp in POr
                from op in LOGICAL_OR
                from andexp in PAnd
                select (Expression)new Binary(op.Value, orexp, andexp);

            PExp.Rule = POr;

            PExpList.Rule =
                from list in PExp.Many(COMMA)
                select list.ToArray();


            return PProgram;
        }
    }
}
