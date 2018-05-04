using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoAutoGen
{
    public class LevenshteinDistance
    {
        /// <summary>
        /// 取最小的一位數
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="third"></param>
        /// <returns></returns>
        private int LowerOfThree(int first, int second, int third)
        {
            int min = Math.Min(first, second);
            return Math.Min(min, third);
        }

        private int Levenshtein_Distance(string str1, string str2)
        {
            int[,] Matrix;
            int n = str1.Length;
            int m = str2.Length;

            int temp = 0;
            char ch1;
            char ch2;
            int i = 0;
            int j = 0;
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {

                return n;
            }
            Matrix = new int[n + 1, m + 1];

            for (i = 0; i <= n; i++)
            {
                //初始化第一列
                Matrix[i, 0] = i;
            }

            for (j = 0; j <= m; j++)
            {
                //初始化第一行
                Matrix[0, j] = j;
            }

            for (i = 1; i <= n; i++)
            {
                ch1 = str1[i - 1];
                for (j = 1; j <= m; j++)
                {
                    ch2 = str2[j - 1];
                    if (ch1.Equals(ch2))
                    {
                        temp = 0;
                    }
                    else
                    {
                        temp = 1;
                    }
                    Matrix[i, j] = LowerOfThree(Matrix[i - 1, j] + 1, Matrix[i, j - 1] + 1, Matrix[i - 1, j - 1] + temp);
                }
            }
            for (i = 0; i <= n; i++)
            {
                for (j = 0; j <= m; j++)
                {
                    //Console.Write(" {0} ", Matrix[i, j]);
                }
                //Console.WriteLine("");
            }

            return Matrix[n, m];
        }

        /// <summary>
        /// 計算字串相似度
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns></returns>
        public decimal LevenshteinDistancePercent(string input, string compare)
        {
            //int maxLenth = str1.Length > str2.Length ? str1.Length : str2.Length;
            ////////////////////////////////////////////////////////////
            //str1=input str2=compare value
            //考慮到輸入名長度同對比不一致，簡單將對比長度限制到輸入長度
            string str1 = input;
            string str2;
            if (input.Length < compare.Length)
            {
                str2 = compare.Substring(0, input.Length);
            }
            else
            {
                str2 = compare;
            }
            //////////////////////////////////////////////////////////
            int val = Levenshtein_Distance(str1, str2);
            return 1 - (decimal)val / Math.Max(str1.Length, str2.Length);
        }
    }
}
