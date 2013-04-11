using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Intermediate
{
    public enum UnaryOperator
    {
        Unknown,
        Minus,
        Address,
        Dereference
    }
    public class UnaryExpression : Expression
    {
        public Operand Operand { get; set; }
        public UnaryOperator Operator { get; set; }

    }
}
