using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers
{
    public enum CompilationStage
    {
        None,
        PreProcessing,
        Scanning,
        Parsing,
        SemanticAnalysis,
        CodeGeneration,
        PostProcessing,
        Other
    }
}
