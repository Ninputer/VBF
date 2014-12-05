// Copyright 2012 Fan Shi
// 
// This file is part of the VBF project.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace VBF.Compilers.Common
{
    internal class EditDistanceCalculator
    {
        private string m_string1;
        private string m_string2;

        public EditDistanceCalculator(string string1, string string2)
        {
            if (string1.Length > string2.Length)
            {
                m_string1 = string1;
                m_string2 = string2;
            }
            else
            {
                m_string2 = string1;
                m_string1 = string2;
            }
        }

        public int Calculate()
        {
            int[] last = new int[m_string1.Length + 1];
            int[] current = new int[m_string1.Length + 1];

            //initialize last (row[0])
            for (int i = 0; i <= m_string1.Length; i++)
            {
                last[i] = i;
            }

            for (int i = 1; i <= m_string2.Length; i++)
            {
                current[0] = i;

                for (int j = 1; j <= m_string1.Length; j++)
                {
                    int prevcolumn = current[j - 1] + 1;
                    int prevrow = last[j] + 1;
                    int prevboth = last[j - 1] +  (m_string1[j - 1] == m_string2[i - 1] ? 0 : 1);

                    current[j] = Min3(prevcolumn, prevrow, prevboth);
                }

                //swap odd/even
                int[] temp = current;
                current = last;
                last = temp;
            }

            return last[m_string1.Length];
        }

        private static int Min3(int prevcolumn, int prevrow, int prevboth)
        {
            if (prevcolumn < prevrow)
            {
                if (prevcolumn < prevboth)
                {
                    return prevcolumn;
                }
                else
                {
                    return prevboth;
                }
            }
            else
            {
                if (prevrow < prevboth)
                {
                    return prevrow;
                }
                else
                {
                    return prevboth;
                }
            }
        }
    }
}
