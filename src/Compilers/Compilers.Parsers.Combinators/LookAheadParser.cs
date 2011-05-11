using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers.Scanners;

namespace VBF.Compilers.Parsers.Combinators
{
    public class ChooseBestParser<T> : Parser<T>
    {
        public Parser<T> Parser1 { get; private set; }
        public Parser<T> Parser2 { get; private set; }

        public ChooseBestParser(Parser<T> parser1, Parser<T> parser2)
        {
            Parser1 = parser1;
            Parser2 = parser2;
        }

        public override Func<Scanners.ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) =>
            {
                ForkableScanner s1 = scanner;
                ForkableScanner s2 = scanner;
                return context.ChooseBest(Parser1.Run(future)(s1, context), Parser2.Run(future)(s2, context));
            };
        }
    }

    class LookAheadApplier<T, TR> : ILookAheadConverter<T, LookAhead<TR>>
    {
        public Func<Parser<T>, Parser<TR>> Converter { get; private set; }

        public LookAheadApplier(Func<Parser<T>, Parser<TR>> converter)
        {
            Converter = converter;
        }

        LookAhead<TR> ILookAheadConverter<T, LookAhead<TR>>.ConvertShift(Parser<T> parser, Dictionary<int, LookAhead<TR>> choices)
        {
            return new Shift<TR>(Converter(parser), choices);
        }

        LookAhead<TR> ILookAheadConverter<T, LookAhead<TR>>.ConvertSplit(LookAhead<TR> shift, LookAhead<TR> reduce)
        {
            return new Split<TR>(shift, reduce);
        }

        LookAhead<TR> ILookAheadConverter<T, LookAhead<TR>>.ConvertReduce(Parser<T> parser)
        {
            return new Reduce<TR>(Converter(parser));
        }

        LookAhead<TR> ILookAheadConverter<T, LookAhead<TR>>.ConvertFound(Parser<T> parser, LookAhead<TR> next)
        {
            return new Found<TR>(Converter(parser), next);
        }
    }

    class Choose<T> : ILookAheadConverter<T, Func<ForkableScanner, ParserContext, Parser<T>>>
    {

        Func<ForkableScanner, ParserContext, Parser<T>>
            ILookAheadConverter<T, Func<ForkableScanner, ParserContext, Parser<T>>>.
            ConvertShift(Parser<T> parser, Dictionary<int, Func<ForkableScanner, ParserContext, Parser<T>>> choices)
        {
            return (scanner, context) =>
            {
                var speek = scanner.Fork();

                var lpeek = speek.Read();

                if (lpeek.IsEndOfStream)
                {
                    speek.Close();
                    return parser;
                }

                if (choices.ContainsKey(lpeek.TokenIndex))
                {
                    return choices[lpeek.TokenIndex](speek, context);
                }
                else
                {
                    speek.Close();
                    return parser;
                }
            };
        }

        Func<ForkableScanner, ParserContext, Parser<T>>
            ILookAheadConverter<T, Func<ForkableScanner, ParserContext, Parser<T>>>.
            ConvertSplit(Func<ForkableScanner, ParserContext, Parser<T>> shift, Func<ForkableScanner, ParserContext, Parser<T>> reduce)
        {
            return (s, c) => new ChooseBestParser<T>(reduce(s, c), shift(s, c));
        }

        Func<ForkableScanner, ParserContext, Parser<T>>
            ILookAheadConverter<T, Func<ForkableScanner, ParserContext, Parser<T>>>.
            ConvertReduce(Parser<T> parser)
        {
            return (S, c) => parser;
        }

        Func<ForkableScanner, ParserContext, Parser<T>>
            ILookAheadConverter<T, Func<ForkableScanner, ParserContext, Parser<T>>>.
            ConvertFound(Parser<T> parser, Func<ForkableScanner, ParserContext, Parser<T>> next)
        {
            return (s, c) => parser;
        }
    }

    public class LookAheadParser<T> : Parser<T>
    {
        private static Choose<T> choose = new Choose<T>();
        public LookAhead<T> LookAhead { get; protected set; }

        public LookAheadParser(LookAhead<T> lookAhead)
        {
            LookAhead = lookAhead;
        }

        public LookAheadParser()
        {

        }

        public override Func<ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<T, TFuture> future)
        {
            return (scanner, context) => choose.Convert(LookAhead)(scanner, context).Run(future)(scanner, context);
        }
    }

    public class LookAheadTokenParser : Parser<Lexeme>
    {
        public TokenParser TokenParser { get; private set; }
        public AnyTokenParser Any { get; private set; }

        public LookAheadTokenParser(Token expected, string missingCorrection)
        {
            CodeContract.RequiresArgumentNotNull(expected, "expected");

            TokenParser = new TokenParser(expected, missingCorrection);
            Any = new AnyTokenParser();
        }

        public LookAheadTokenParser(Token expected)
        {
            CodeContract.RequiresArgumentNotNull(expected, "expected");

            TokenParser = new TokenParser(expected);
            Any = new AnyTokenParser();
        }

        public override Func<ForkableScanner, ParserContext, Result<TFuture>> Run<TFuture>(Future<Lexeme, TFuture> future)
        {
            var shift = new Shift<Lexeme>(TokenParser);
            shift.Choices.Add(TokenParser.ExpectedToken.Index, new Reduce<Lexeme>(Any));

            var lookAhead = new Found<Lexeme>(TokenParser, shift);
            LookAheadParser<Lexeme> parser = new LookAheadParser<Lexeme>(lookAhead);

            return parser.Run(future);
        }
    }

    class ConcatConvert<T1, T2, TR> : ILookAheadConverter<T1, LookAhead<TR>>
    {

        public LookAheadParser<T1> ParserLeft { get; private set; }
        public LookAheadParser<T2> ParserRight { get; private set; }
        public Func<T1, T2, TR> Selector { get; private set; }

        public ConcatConvert(LookAheadParser<T1> parserLeft, LookAheadParser<T2> parserRight, Func<T1, T2, TR> resultSelector)
        {
            ParserLeft = parserLeft;
            ParserRight = parserRight;
            Selector = resultSelector;
        }


        public LookAhead<TR> ConvertShift(Parser<T1> parser, Dictionary<int, LookAhead<TR>> choices)
        {
            return new Shift<TR>(new ConcatenationParser<T1, T2, TR>(parser, a => ParserRight, Selector), choices);
        }

        public LookAhead<TR> ConvertSplit(LookAhead<TR> shift, LookAhead<TR> reduce)
        {
            return shift.Merge(reduce);
        }

        public LookAhead<TR> ConvertReduce(Parser<T1> parser)
        {
            Func<Parser<T2>, Parser<TR>> converter = p2 => new ConcatenationParser<T1, T2, TR>(parser, a => p2, Selector);
            LookAheadApplier<T2, TR> reduceConverter = new LookAheadApplier<T2, TR>(converter);

            return reduceConverter.Convert(ParserRight.LookAhead);
        }

        public LookAhead<TR> ConvertFound(Parser<T1> parser, LookAhead<TR> next)
        {
            return new Found<TR>(new ConcatenationParser<T1, T2, TR>(parser, a => ParserRight, Selector), next);
        }
    }

    public static class LookAheadParsers
    {
        public static LookAheadParser<Lexeme> AsLAParser(this Token token)
        {
            CodeContract.RequiresArgumentNotNull(token, "token");

            var tokenParser = new TokenParser(token);
            var any = new AnyTokenParser();

            var shift = new Shift<Lexeme>(tokenParser);
            shift.Choices.Add(tokenParser.ExpectedToken.Index, new Reduce<Lexeme>(any));

            var lookAhead = new Found<Lexeme>(tokenParser, shift);
            LookAheadParser<Lexeme> parser = new LookAheadParser<Lexeme>(lookAhead);

            return parser;
        }

        public static LookAheadParser<Lexeme> Eos(int eosTokenIndex)
        {
            var eosParser = new EndOfStreamParser();

            var shift = new Shift<Lexeme>(eosParser);
            shift.Choices.Add(eosTokenIndex, new Reduce<Lexeme>(eosParser));

            var lookAhead = new Found<Lexeme>(eosParser, shift);
            LookAheadParser<Lexeme> parser = new LookAheadParser<Lexeme>(lookAhead);

            return parser;
        }

        public static LookAheadParser<T> Succeed<T>(T value)
        {
            return new LookAheadParser<T>(new Reduce<T>(new SucceedParser<T>(value)));
        }

        public static LookAheadParser<TResult> SelectMany<T1, T2, TResult>(this LookAheadParser<T1> parser, Func<T1, LookAheadParser<T2>> parserSelector, Func<T1, T2, TResult> resultSelector)
        {
            CodeContract.RequiresArgumentNotNull(parser, "parser");
            CodeContract.RequiresArgumentNotNull(parserSelector, "parserSelector");
            CodeContract.RequiresArgumentNotNull(resultSelector, "resultSelector");

            ConcatConvert<T1, T2, TResult> cconverter = new ConcatConvert<T1, T2, TResult>(parser, parserSelector(default(T1)), resultSelector);

            var lookAhead = cconverter.Convert(parser.LookAhead);

            return new LookAheadParser<TResult>(lookAhead);
        }

        public static LookAheadParser<T> Union<T>(this LookAheadParser<T> parser1, LookAheadParser<T> parser2)
        {
            CodeContract.RequiresArgumentNotNull(parser1, "parser1");
            CodeContract.RequiresArgumentNotNull(parser2, "parser2");

            return new LookAheadParser<T>(parser1.LookAhead.Merge(parser2.LookAhead));
        }
    }

    public class LookAheadParserReference<T> : LookAheadParser<T>
    {
        public LookAheadParser<T> Reference
        {
            set
            {
                LookAhead = value.LookAhead;
            }
        }
    }
}
