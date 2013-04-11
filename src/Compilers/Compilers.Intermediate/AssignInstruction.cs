using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Intermediate
{
    public class AssignInstruction : Instruction
    {
        public Variable Target { get; set; }
    }
}
