using System;
using System.Collections.Generic;
using System.IO;

namespace Heuristics
{
    public class DynamicAlgorithm1
    {
        public static void Execute(string folderPath)
        {
            int nc = 0;
            int np = 0;
            int nv = 0;

            string[] lines = File.ReadAllLines(folderPath + "");

            int lineCounter = 0;
            string auxStr = string.Empty;
            foreach (string line in lines) 
            {
                for (int i = 0; i < line.Length; i++)
                    if (Char.IsDigit(line[i]))
                        auxStr += line[i];
                if (auxStr.Length > 0)
                { 
                    if(lineCounter == 0)
                        nc = int.Parse(auxStr);
                    else if(lineCounter == 1)
                        np = int.Parse(auxStr);
                    else if (lineCounter == 1)
                        nv = int.Parse(auxStr);
                }   
                lineCounter++;
            }

            int[,] tt = new int[np, nc];
            int[,] cc = new int[np, nc];
            int[] s = new int[nc];
            int[] cfr = new int[nc];
            int[] vold = new int[nc];
            int[] codLoadingPlants = new int[np];
            int[] codOrders = new int[nc];
            int[] codDeliveries = new int[nc];

            //int[,,,,] t = new int[np, nc, nc, nc, nc];
            //int[,,,,] c = new int[np, nc, nc, nc, nc];
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>> c = 
                new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>>();
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>> t = 
                new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>>();

            for (int p = 0; p < np; p++)
            {
                for (int i = 0; i < np; i++)
                {
                    for (int j = 0; j < np; j++)
                    {
                        for (int k = 0; k < np; k++)
                        {
                            for (int l = 0; l < np; l++)
                            {
                                if (!c.ContainsKey(p))
                                    c[p] = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>();
                                if (!c[p].ContainsKey(i))
                                    c[p][i] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                if (!c[p][i][j].ContainsKey(k))
                                    c[p][i][j][k] = new Dictionary<int, double>();

                                if (!t.ContainsKey(p))
                                    t[p] = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>();
                                if (!t[p].ContainsKey(i))
                                    t[p][i] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                if (!t[p][i][j].ContainsKey(k))
                                    t[p][i][j][k] = new Dictionary<int, double>();

                                if (i != j && i != k && i != l && j != k && j != l && k != l &&
                                    (s[i] < s[j]) && (s[j] < s[k]) && (s[k] < s[l]))
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, i] + cc[p, j] + cc[p, k] + cc[p, l]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, i]) + (2 * tt[p, j]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                            (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 40;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                                else if (i == j && k != i && l != i && k != l && (s[j] < s[k]) && (s[k] < s[l]))// 1 1 x x
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, i] + cc[p, k] + cc[p, l]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, i]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                            (cfr[i] * vold[i]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 30;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                                else if (j == k && i != j && l != j && l != i && (s[i] < s[j]) && (s[j] < s[l])) // x 1 1 x
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, i] + cc[p, k] + cc[p, l]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, i]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                            (cfr[i] * vold[i]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 30;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                                else if (k == l && i != k && j != k && i != j && (s[i] < s[j]) && (s[j] < s[l])) // x x 1 1
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, i] + cc[p, j] + cc[p, k]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, i]) + (2 * tt[p, j]) + (2 * tt[p, k]) +
                                            (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + 30;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                                else if (i == j && j == k && k != l && (s[k] < s[l])) // 1 1 1 x
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, k] + cc[p, l]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, k]) + (2 * tt[p, l]) +
                                            (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 20;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                                else if (j == k && k == l && i != j && (s[i] < s[j])) // x 1 1 1
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, i] + cc[p, j]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, i]) + (2 * tt[p, j]) +
                                            (cfr[j] * vold[j]) + (cfr[j] * vold[j]) + 20;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                                else if (i == j && k == l && j != k && (s[j] < s[k])) // 1 1 0 0
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, j] + cc[p, k]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, j]) + (2 * tt[p, k]) +
                                            (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + 20;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                                else if (i == j && j == k && k == l) // 1 1 1 1 
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, i]);

                                    if (!t[p][i][j][k].ContainsKey(l))
                                    {
                                        double tval = (2 * tt[p, i]) + (cfr[i] * vold[i]) + 10;
                                        t[p][i][j][k].Add(l, tval);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
