using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Intermediate
{
    public enum BinaryOperator
    {
        Unknown,
        Add,
        Substract,
        Multiply,
        Divide,
        Modulo,
        Min,
        Max,
        Member,
        DereferenceMember
    }

    public class BinaryExpression : Expression
    {
        public Operand Operand1 { get; set; }
        public Operand Operand2 { get; set; }

        public BinaryOperator Operator { get; set; }

    }
}
