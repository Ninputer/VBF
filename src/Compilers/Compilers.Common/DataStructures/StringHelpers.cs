using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VBF.Compilers.DataStructures
{
    public static class StringHelpers
    {
        public static int EditDistance(string str1, string str2)
        {
            var calc = new EditDistanceCalculator(str1, str2);

            return calc.Calculate();
        }
    }
}
