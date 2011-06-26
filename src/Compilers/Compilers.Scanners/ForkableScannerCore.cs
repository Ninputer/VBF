using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VBF.Compilers.Scanners
{
    class ForkableScannerCore
    {
        internal readonly CacheQueue<Lexeme> LookAheadQueue;
        internal readonly Scanner MasterScanner;

        internal ForkableScannerCore(Scanner masterScanner)
        {
            MasterScanner = masterScanner;
            LookAheadQueue = new CacheQueue<Lexeme>();
        }
    }
}
