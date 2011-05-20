using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.MiniSharp
{
    class ProgramEntry
    {
        static void Main(string[] args)
        {
            string source = @"
class ProgramEntry
{
    //the main method
    public static void Main(string[] args)
    {
        //hello world
    }  
}
";
            MiniSharpParser p = new MiniSharpParser();
            var ast = p.Parse(source);

            ;
        }
    }
}
