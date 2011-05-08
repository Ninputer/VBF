using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers
{
    public class CompilationErrorInfo
    {
        public int Id { get; private set; }
        public int Level { get; private set; }
        public CompilationStage Stage { get; private set; }
        public string MessageTemplate { get; set; }

        public CompilationErrorInfo(int id, int level, CompilationStage stage, string messageTemplate)
        {
            Id = id;
            Level = level;
            Stage = stage;
            MessageTemplate = messageTemplate;
        }
    }
}
