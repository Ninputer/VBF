using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VBF.Compilers;
using System.Diagnostics;


namespace VBF.MiniSharp
{
    class ProgramEntry
    {
        static void Main(string[] args)
        {
            string source = @"
static class 程序入口
{
    //中文注释
    public static void Main(string[] args)
    {
        Fac o;
        o = new Fac();
        System.Console.WriteLine(o.ComputeFac(8));
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

    public int Foo()
    {
        return this.Foo(a);
    }
}

";

            Stopwatch sw = new Stopwatch();
            sw.Start();

            CompilationErrorManager errorManager = new CompilationErrorManager();
            MiniSharpParser p = new MiniSharpParser(errorManager);
            p.Initialize();

            sw.Stop();
            Console.WriteLine("Initialize time: {0} ms", sw.ElapsedMilliseconds);
            sw.Restart();

            var ast = p.Parse(source);

            sw.Stop();
            Console.WriteLine("Parsing time: {0} ms", sw.ElapsedMilliseconds);
            sw.Restart();

            if (errorManager.Errors.Count != 0)
            {
                ReportErrors(errorManager);
                return;
            }

            TypeDeclResolver resolver1 = new TypeDeclResolver(errorManager);
            resolver1.DefineErrors();
            resolver1.Visit(ast);

            MemberDeclResolver resolver2 = new MemberDeclResolver(errorManager, resolver1.Types);
            resolver2.DefineErrors();
            resolver2.Visit(ast);

            MethodBodyResolver resolver3 = new MethodBodyResolver(errorManager, resolver1.Types);
            resolver3.DefineErrors();
            resolver3.Visit(ast);

            sw.Stop();
            Console.WriteLine("Semantic analysis time: {0} ms", sw.ElapsedMilliseconds);

            if (errorManager.Errors.Count != 0)
            {
                ReportErrors(errorManager);
                return;
            }

            //generate Cil
            var codegenDomain = AppDomain.CurrentDomain;
            var cilTrans = new VBF.MiniSharp.Targets.Cil.EmitTranslator(codegenDomain, "test");

            cilTrans.Create(ast, @"test.exe");

            ;
        }

        private static void ReportErrors(CompilationErrorManager errorManager)
        {
            if (errorManager.Errors.Count > 0)
            {
                foreach (var error in errorManager.Errors.OrderBy(e => e.ErrorPosition.StartLocation.CharIndex))
                {
                    Console.WriteLine(error.ToString());
                }
            }
        }
    }
}
