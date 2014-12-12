// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VBF.Compilers;
using VBF.Compilers.Parsers.Combinators;
using VBF.Compilers.Scanners;
using VBF.MiniSharp.Ast;
using RE = VBF.Compilers.Scanners.RegularExpression;
using Type = VBF.MiniSharp.Ast.Type;

namespace VBF.MiniSharp.CombinatorParser
{
    public class MiniSharpParser : ParserFrame<Program>
    {
        //keywords
        private Token ASSIGN; // =
        private Token ASTERISK; // *
        private Token COLON; // :
        private Token COMMA; // ,
        private Token COMMENT;
        private Token DOT; // .
        private Token EQUAL; // ==
        private Token GREATER; // >
        private Token ID; //identifier
        private Token INTEGER_LITERAL; //integer literal
        private Token K_BOOL;
        private Token K_CLASS;
        private Token K_CONSOLE;
        private Token K_ELSE;
        private Token K_FALSE;
        private Token K_IF;
        private Token K_INT;
        private Token K_LENGTH;
        private Token K_MAIN;
        private Token K_NEW;
        private Token K_PUBLIC;
        private Token K_RETURN;
        private Token K_STATIC;
        private Token K_STRING;
        private Token K_SYSTEM;
        private Token K_THIS;
        private Token K_TRUE;
        private Token K_VOID;
        private Token K_WHILE;
        private Token K_WRITELINE;
        private Token LEFT_BK; // [
        private Token LEFT_BR; // {
        private Token LEFT_PH; // (
        private Token LESS; // <
        private Token LINE_BREAKER;
        private Token LOGICAL_AND; // &&
        private Token LOGICAL_NOT; // !
        private Token LOGICAL_OR; // ||
        private Token MINUS; // -
        private ParserReference<Expression> PAnd = new ParserReference<Expression>();
        private ParserReference<Statement> PArrayAssignment = new ParserReference<Statement>();
        private ParserReference<Func<Expression, Expression>> PArrayLength = new ParserReference<Func<Expression, Expression>>();
        private ParserReference<Func<Expression, Expression>> PArrayLookup = new ParserReference<Func<Expression, Expression>>();
        private ParserReference<Statement> PAssignment = new ParserReference<Statement>();
        private ParserReference<Expression> PBoolLiteral = new ParserReference<Expression>();
        private ParserReference<Type> PBoolType = new ParserReference<Type>();
        private ParserReference<Func<Expression, Expression>> PCall = new ParserReference<Func<Expression, Expression>>();

        //grammar
        private ParserReference<ClassDecl> PClassDecl = new ParserReference<ClassDecl>();
        private ParserReference<Expression> PComparand = new ParserReference<Expression>();
        private ParserReference<Expression> PComparison = new ParserReference<Expression>();
        private ParserReference<Expression> PExp = new ParserReference<Expression>();
        private ParserReference<Expression[]> PExpList = new ParserReference<Expression[]>();
        private ParserReference<Expression> PFactor = new ParserReference<Expression>();
        private ParserReference<FieldDecl> PFieldDecl = new ParserReference<FieldDecl>();
        private ParserReference<Formal[]> PFormalList = new ParserReference<Formal[]>();
        private ParserReference<Type> PIdType = new ParserReference<Type>();
        private ParserReference<Statement> PIfElse = new ParserReference<Statement>();
        private ParserReference<Type> PIntArrayType = new ParserReference<Type>();
        private ParserReference<Type> PIntType = new ParserReference<Type>();
        private Token PLUS; // +
        private ParserReference<MainClass> PMainClass = new ParserReference<MainClass>();
        private ParserReference<MethodDecl> PMethodDecl = new ParserReference<MethodDecl>();
        private ParserReference<Expression> PNewArray = new ParserReference<Expression>();
        private ParserReference<Expression> PNewObj = new ParserReference<Expression>();
        private ParserReference<Expression> PNot = new ParserReference<Expression>();
        private ParserReference<Expression> PNumberLiteral = new ParserReference<Expression>();
        private ParserReference<Expression> POr = new ParserReference<Expression>();
        private ParserReference<Program> PProgram = new ParserReference<Program>();
        private ParserReference<Statement> PStatement = new ParserReference<Statement>();
        private ParserReference<Expression> PTerm = new ParserReference<Expression>();
        private ParserReference<Expression> PThis = new ParserReference<Expression>();
        private ParserReference<Type> PType = new ParserReference<Type>();
        private ParserReference<Statement> PVarDeclStmt = new ParserReference<Statement>();
        private ParserReference<Expression> PVariable = new ParserReference<Expression>();
        private ParserReference<Statement> PWhile = new ParserReference<Statement>();
        private ParserReference<Statement> PWriteLine = new ParserReference<Statement>();
        private Token RIGHT_BK; // ]
        private Token RIGHT_BR; // }
        private Token RIGHT_PH; // )
        private Token SEMICOLON; // ;
        private Token SLASH; // /
        private Token WHITESPACE;
        public MiniSharpParser(CompilationErrorManager errorManager) : base(errorManager, 101, 201, 202) { }

        protected override void OnDefineLexer(Lexicon lexicon, ICollection<Token> skippedTokens)
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

        protected override Parser<Program> OnDefineParser()
        {
            PProgram.Reference = // MainClass ClassDecl*
                from main in PMainClass
                from classes in PClassDecl.Many()
                select new Program(main, classes.ToArray());

            PMainClass.Reference = // static class id { public static void Main(string[] id) { statement }}
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

            PClassDecl.Reference = //class id
                from _class in K_CLASS
                from className in ID
                from def in (classDeclSimple | classDeclInherits)
                select new ClassDecl(className.Value, def.BaseClassName.GetValue(), def.Members.Fields, def.Members.Methods);

            PFieldDecl.Reference = // Type id;
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

            PMethodDecl.Reference = // public Type id (FormalList) { Statement* return Exp; }
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

            PFormalList.Reference = // Type id FormalRest* | <empty>
                 from list in paramFormal.Many(COMMA)
                 select list.ToArray();

            PType.Reference = // int[] | bool | int | id 
                PIntArrayType | PBoolType | PIntType | PIdType;

            PIntArrayType.Reference = //int[]
                from _int in K_INT
                from _lb in LEFT_BK
                from _rb in RIGHT_BK
                select (Type)new IntArrayType();

            PBoolType.Reference = // bool
                from _bool in K_BOOL
                select (Type)new BooleanType();

            PIntType.Reference = // int
                from _int in K_INT
                select (Type)new IntegerType();

            PIdType.Reference = // id
                from type in ID
                select (Type)new IdentifierType(type.Value);

            //statements

            PStatement.Reference = // { statement*} | ifelse | while | writeline | assign | array assign | var decl
                (from _1 in LEFT_BR from stmts in PStatement.Many() from _2 in RIGHT_BR select (Statement)new Block(stmts.ToArray())) |
                PIfElse |
                PWhile |
                PWriteLine |
                PAssignment |
                PArrayAssignment |
                PVarDeclStmt;

            PIfElse.Reference = // if ( exp ) statement else statement
                from _if in K_IF
                from _1 in LEFT_PH
                from condExp in PExp
                from _2 in RIGHT_PH
                from truePart in PStatement
                from _else in K_ELSE
                from falsePart in PStatement
                select (Statement)new IfElse(condExp, truePart, falsePart, _if.Value.Span, _else.Value.Span);

            PWhile.Reference = // while ( exp ) statement
                from _while in K_WHILE
                from _1 in LEFT_PH
                from condExp in PExp
                from _2 in RIGHT_PH
                from loopBody in PStatement
                select (Statement)new While(condExp, loopBody, _while.Value.Span);

            PWriteLine.Reference = // System.Console.WriteLine( exp );
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

            PAssignment.Reference = // id = exp;
                from variable in ID
                from _eq in ASSIGN
                from value in PExp
                from _sc in SEMICOLON
                select (Statement)new Assign(variable.Value, value);

            PArrayAssignment.Reference = // id[ exp ] = exp ;
                from variable in ID
                from _1 in LEFT_BK
                from index in PExp
                from _2 in RIGHT_BK
                from _eq in ASSIGN
                from value in PExp
                from _sc in SEMICOLON
                select (Statement)new ArrayAssign(variable.Value, index, value);

            PVarDeclStmt.Reference = // Type id;
                from type in PType
                from varName in ID
                from _sc in SEMICOLON
                select (Statement)new VarDecl(type, varName.Value);

            //expressions

            //basic
            PNumberLiteral.Reference = // number
                from intvalue in INTEGER_LITERAL
                select (Expression)new IntegerLiteral(intvalue.Value);

            PBoolLiteral.Reference = // true | false
                from b in K_TRUE.AsParser() | K_FALSE.AsParser()
                select (Expression)new BooleanLiteral(b.Value);

            PThis.Reference = // this
                from _this in K_THIS
                select (Expression)new This(_this.Value.Span);

            PVariable.Reference = // id
                from varName in ID
                select (Expression)new Variable(varName.Value);

            PNewObj.Reference = // new id()
                from _new in K_NEW
                from typeName in ID
                from _1 in LEFT_PH
                from _2 in RIGHT_PH
                select (Expression)new NewObject(typeName.Value);

            PNewArray.Reference = // new int [exp]
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
                PExp.PackedBy(LEFT_PH.AsParser(), RIGHT_PH.AsParser());


            PCall.Reference = // exp.id(explist)
                from _d in DOT
                from methodName in ID
                from _1 in LEFT_PH
                from args in PExpList
                from _2 in RIGHT_PH
                select new Func<Expression, Expression>(e =>
                    new Call(e, methodName.Value, args));

            PArrayLookup.Reference = // exp[exp]
                from _1 in LEFT_BK
                from index in PExp
                from _2 in RIGHT_BK
                select new Func<Expression, Expression>(e =>
                    new ArrayLookup(e, index, new SourceSpan(_1.Value.Span.EndLocation, _2.Value.Span.StartLocation)));

            PArrayLength.Reference = // exp.Length
                from _d in DOT
                from _length in K_LENGTH
                select new Func<Expression, Expression>(e =>
                    new ArrayLength(e, _length.Value.Span));

            var basicExp = //foundation >> call | id[exp] | id.Length
                from exp in foundationExp
                from follow in (PCall | PArrayLookup | PArrayLength).Optional()
                select follow == null ? exp : follow(exp);

            //unary 

            PNot.Reference = // ! exp
                basicExp |
                from _n in LOGICAL_NOT
                from exp in PNot
                select (Expression)new Not(exp, _n.Value.Span);

            //binary

            PFactor.Reference = // exp | !exp
                PNot;

            var termRest =
                from op in (ASTERISK.AsParser() | SLASH.AsParser())
                from factor in PFactor
                select new { Op = op, Right = factor };

            PTerm.Reference = // term * factor | factor
                from factor in PFactor
                from rest in termRest.Many()
                select rest.Aggregate(factor, (f, r) => new Binary(r.Op.Value, f, r.Right));

            var comparandRest =
                from op in (PLUS.AsParser() | MINUS.AsParser())
                from term in PTerm
                select new { Op = op, Right = term };

            PComparand.Reference = // comparand + term | term
                from term in PTerm
                from rest in comparandRest.Many()
                select rest.Aggregate(term, (t, r) => new Binary(r.Op.Value, t, r.Right));


            var comparisonRest =
                from op in (LESS.AsParser() | GREATER.AsParser() | EQUAL.AsParser())
                from comparand in PComparand
                select new { Op = op, Right = comparand };

            PComparison.Reference = // comparison < comparand | comparand
                from comparand in PComparand
                from rest in comparisonRest.Many()
                select rest.Aggregate(comparand, (c, r) => new Binary(r.Op.Value, c, r.Right));

            var andRest =
                from op in LOGICAL_AND
                from comparison in PComparison
                select new { Op = op, Right = comparison };

            PAnd.Reference = // andexp && comparison | comparison
                from comparison in PComparison
                from rest in andRest.Many()
                select rest.Aggregate(comparison, (c, r) => new Binary(r.Op.Value, c, r.Right));

            var orRest =
                from op in LOGICAL_OR
                from comparison in PComparison
                select new { Op = op, Right = comparison };

            POr.Reference = // orexp || andexp | andexp
                from and in PAnd
                from rest in orRest.Many()
                select rest.Aggregate(and, (c, r) => new Binary(r.Op.Value, c, r.Right));

            PExp.Reference = POr;

            PExpList.Reference =
                from list in PExp.Many(COMMA)
                select list.ToArray();


            return PProgram;
        }
    }
}
