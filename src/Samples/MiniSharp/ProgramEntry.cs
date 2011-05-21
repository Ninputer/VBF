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
class 程序入口
{
    //中文注释
    public static void Main(string[] args)
    {
        //hello world
        Fac o;
        o = new Fac();
        System.Console.WriteLine(o.ComputeFac(123));
    }
}

class Fac
{
    public int ComputeFac(int num)
    {
        int num_aux;
        if (num < 1)
            num_aux = 1;
        else
            num_aux = num * (this.ComputeFac(num - 1));
        return num_aux;
    }
}
";
            MiniSharpParser p = new MiniSharpParser();
            var ast = p.Parse(source);

            ;
        }
    }
}
