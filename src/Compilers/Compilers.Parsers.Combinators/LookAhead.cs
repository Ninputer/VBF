using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Parsers.Combinators
{
    public abstract class LookAhead<T>
    {
        public LookAheadType Type { get; private set; }
        public Parser<T> Parser { get; private set; }

        protected LookAhead(LookAheadType type, Parser<T> parser)
        {
            Type = type;
            Parser = parser;
        }

        public abstract LookAhead<T> Merge(LookAhead<T> other);
    }

    public class Shift<T> : LookAhead<T>
    {
        private Dictionary<int, LookAhead<T>> m_choices;

        public Shift(Parser<T> parser)
            : base(LookAheadType.Shift, parser)
        {
            m_choices = new Dictionary<int, LookAhead<T>>();
        }

        public Shift(Dictionary<int, LookAhead<T>> choices, Parser<T> parser)
            : base(LookAheadType.Shift, parser)
        {
            m_choices = new Dictionary<int, LookAhead<T>>(choices);
        }

        private void Combine(Dictionary<int, LookAhead<T>> otherChoices)
        {

            foreach (var choice in otherChoices)
            {
                if (!m_choices.ContainsKey(choice.Key))
                {
                    m_choices.Add(choice.Key, choice.Value);
                }
                else
                {
                    var exist = m_choices[choice.Key];
                    var merged_choice = exist.Merge(choice.Value);

                    m_choices[choice.Key] = merged_choice;
                }
            }
        }

        public override LookAhead<T> Merge(LookAhead<T> other)
        {
            switch (other.Type)
            {
                case LookAheadType.Shift:
                    var newshift = new Shift<T>(this.m_choices, new ChooseBestParser<T>(this.Parser, other.Parser));
                    newshift.Combine(((Shift<T>)other).m_choices);
                    return newshift;

                case LookAheadType.Split:
                    var split = (Split<T>)other;

                    var shift = this.Merge(split.Shift);
                    return new Split<T>(shift, split.Reduce);

                case LookAheadType.Reduce:
                    return new Split<T>(this, other);

                case LookAheadType.Found:
                    var found = (Found<T>)other;
                    return this.Merge(found.Next);

                default:
                    throw new ArgumentException("LookAhead type is invalid");
            }
            throw new NotImplementedException();
        }
    }

    public class Split<T> : LookAhead<T>
    {

        public LookAhead<T> Shift { get; private set; }
        public LookAhead<T> Reduce { get; private set; }


        public Split(LookAhead<T> shift, LookAhead<T> reduce)
            : base(LookAheadType.Split, null)
        {
            Shift = shift;
            Reduce = reduce;
        }

        public override LookAhead<T> Merge(LookAhead<T> other)
        {
            switch (other.Type)
            {
                case LookAheadType.Shift:
                    return other.Merge(this);
                
                default:
                    throw new ArgumentException("Ambiguous grammer: trying to merge a Split with a LookAhead rather than Shift");
            }
        }
    }

    public class Reduce<T> : LookAhead<T>
    {
        public Reduce(Parser<T> parser) : base(LookAheadType.Reduce, parser) { }

        public override LookAhead<T> Merge(LookAhead<T> other)
        {
            switch (other.Type)
            {
                case LookAheadType.Shift:
                    return other.Merge(this);

                default:
                    throw new ArgumentException("Ambiguous grammer: trying to merge a Reduce with a LookAhead rather than Shift");
            }
        }
    }

    public class Found<T> : LookAhead<T>
    {
        public LookAhead<T> Next { get; set; }

        public Found(Parser<T> parser) : base(LookAheadType.Found, parser) { }

        public override LookAhead<T> Merge(LookAhead<T> other)
        {
            return Next.Merge(other);
        }
    }
}
