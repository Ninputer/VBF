using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers.Combinators
{
    public interface ILookAheadConverter<T, TResult>
    {
        TResult ConvertShift(Parser<T> parser, Dictionary<int, TResult> choices);

        TResult ConvertSplit(TResult shift, TResult reduce);

        TResult ConvertReduce(Parser<T> parser);

        TResult ConvertFound(Parser<T> parser, TResult next);
    }

    public static class LookAheadConverter
    {
        public static TResult Visit<T, TResult>(this ILookAheadConverter<T, TResult> visitor, LookAhead<T> lookAhead)
        {
            switch (lookAhead.Type)
            {
                case LookAheadType.Shift:
                    var shift = (Shift<T>)lookAhead;
                    var newchoices = new Dictionary<int, TResult>();
                    foreach (var choice in shift.Choices)
                    {
                        newchoices[choice.Key] = visitor.Visit(choice.Value);
                    }

                    return visitor.ConvertShift(shift.Parser, newchoices);
                case LookAheadType.Split:
                    var split = (Split<T>)lookAhead;

                    return visitor.ConvertSplit(visitor.Visit(split.Shift), visitor.Visit(split.Reduce));
                case LookAheadType.Reduce:
                    var reduce = (Reduce<T>)lookAhead;

                    return visitor.ConvertReduce(reduce.Parser);
                case LookAheadType.Found:
                    var found = (Found<T>)lookAhead;

                    return visitor.ConvertFound(found.Parser, visitor.Visit(found.Next));
                default:
                    throw new ArgumentException("The Type of LookAhead is invalid", "lookAhead");
            }
        }
    }
}
