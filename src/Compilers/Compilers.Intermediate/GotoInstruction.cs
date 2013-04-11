using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.Intermediate
{
    public class GotoInstruction : Instruction
    {
        public Label Location { get; set; }
    }
}
