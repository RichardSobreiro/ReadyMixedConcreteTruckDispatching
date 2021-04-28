using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Heuristics
{
    public class DynamicAlgorithm1
    {
        public static void Execute(string folderPath)
        {
            int nc = 0;
            int np = 0;
            int nv = 0;

            string[] lines = File.ReadAllLines(folderPath + "\\BianchessiReal.dat");

            int lineCounter = 1;
            int nptt = 0;
            int npcc = 0;
            foreach (string line in lines)
            {
                if (lineCounter <= 3)
                {
                    string auxStr = string.Empty;
                    for (int i = 0; i < line.Length; i++)
                        if (Char.IsDigit(line[i]))
                            auxStr += line[i];
                    if (auxStr.Length > 0)
                    {
                        if (lineCounter == 1)
                            nc = int.Parse(auxStr);
                        else if (lineCounter == 2)
                            np = int.Parse(auxStr);
                        else if (lineCounter == 3)
                            nv = int.Parse(auxStr);
                    }
                }
                else
                {
                    break;
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
            lineCounter = 0;
            foreach (string line in lines)
            {
                if (lineCounter > 4 && lineCounter <= (4 + np))
                { 
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    var values = val.Split(',');
                    int nvtt = 0;
                    foreach (var value in values)
                    {
                        if(!string.IsNullOrEmpty(value))
                        {
                            tt[nptt, nvtt] = int.Parse(value);
                            nvtt++;
                        }
                    }
                    nptt++;
                }
                else if (lineCounter > (4 + np + 2) && lineCounter <= (4 + np + 2 + np))
                {
                    var values = line.Remove('[').Remove(']').Split(',');
                    int nvcc = 0;
                    foreach (var value in values)
                    {
                        cc[npcc, nvcc] = int.Parse(value);
                        nvcc++;
                    }
                    npcc++;
                }
                else if (lineCounter == (4 + np + 2 + np + 2))
                {
                    var values = line.Remove('s').Remove('=').Remove(' ').Remove('[').Remove(']').Split(',');
                    int nvs = 0;
                    foreach (var value in values)
                    {
                        s[nvs] = int.Parse(value);
                        nvs++;
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 3))
                {
                    var values = line.Remove('c').Remove('f').Remove('r').Remove('=').Remove(' ').Remove('[').Remove(']').Split(',');
                    int nvcfr = 0;
                    foreach (var value in values)
                    {
                        cfr[nvcfr] = int.Parse(value);
                        nvcfr++;
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 4))
                {
                    var values = line.Remove('v').Remove('o').Remove('l').Remove('d').Remove('=').Remove(' ').Remove('[').Remove(']').Split(',');
                    int nvvold = 0;
                    foreach (var value in values)
                    {
                        vold[nvvold] = int.Parse(value);
                        nvvold++;
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 5))
                {
                    int index = line.IndexOf("codLoadingPlants");
                    string cleanPath = (index < 0)
                        ? line
                        : line.Remove(index, "codLoadingPlants".Length);
                    var values = cleanPath.Remove('=').Remove(' ').Remove('[').Remove(']').Split(',');
                    int nvvold = 0;
                    foreach (var value in values)
                    {
                        codLoadingPlants[nvvold] = int.Parse(value);
                        nvvold++;
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 6))
                {
                    int index = line.IndexOf("codOrders");
                    string cleanPath = (index < 0)
                        ? line
                        : line.Remove(index, "codOrders".Length);
                    var values = cleanPath.Remove('=').Remove(' ').Remove('[').Remove(']').Split(',');
                    int nvcodOrders = 0;
                    foreach (var value in values)
                    {
                        codOrders[nvcodOrders] = int.Parse(value);
                        nvcodOrders++;
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 7))
                {
                    int index = line.IndexOf("codDeliveries");
                    string cleanPath = (index < 0) ? line : line.Remove(index, "codDeliveries".Length);
                    var values = cleanPath.Remove('=').Remove(' ').Remove('[').Remove(']').Split(',');
                    int nvcodDeliveries = 0;
                    foreach (var value in values)
                    {
                        codDeliveries[nvcodDeliveries] = int.Parse(value);
                        nvcodDeliveries++;
                    }
                }
                lineCounter++;
            }

            //int[,,,,] t = new int[np, nc, nc, nc, nc];
            //int[,,,,] c = new int[np, nc, nc, nc, nc];
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>> c = 
                new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>>();
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>> t = 
                new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>>();

            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>> fourCustomers =
                new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>>();
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>> threeCustomers =
                new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>();
            Dictionary<int, Dictionary<int, Dictionary<int, double>>> twoCustomers = 
                new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
            Dictionary<int, Dictionary<int, double>> oneCustomers = 
                new Dictionary<int, Dictionary<int, double>>();

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
                                if (!c[p][i].ContainsKey(j))
                                    c[p][i][j] = new Dictionary<int, Dictionary<int, double>>();
                                if (!c[p][i][j].ContainsKey(k))
                                    c[p][i][j][k] = new Dictionary<int, double>();

                                if (!t.ContainsKey(p))
                                    t[p] = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>();
                                if (!t[p].ContainsKey(i))
                                    t[p][i] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                if (!t[p][i].ContainsKey(j))
                                    t[p][i][j] = new Dictionary<int, Dictionary<int, double>>();
                                if (!t[p][i][j].ContainsKey(k))
                                    t[p][i][j][k] = new Dictionary<int, double>();

                                if (i != j && i != k && i != l && j != k && j != l && k != l &&
                                    (s[i] < s[j]) && (s[j] < s[k]) && (s[k] < s[l]))
                                {
                                    c[p][i][j][k].Add(l, cc[p, i] + cc[p, j] + cc[p, k] + cc[p, l]);
                                    double tval = (2 * tt[p, i]) + (2 * tt[p, j]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 40;
                                    t[p][i][j][k].Add(l, tval);

                                    if (!fourCustomers.ContainsKey(p))
                                        fourCustomers[p] = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, double>>>>();
                                    if (!fourCustomers[p].ContainsKey(i))
                                        fourCustomers[p][i] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                    if (!fourCustomers[p][i].ContainsKey(j))
                                        fourCustomers[p][i][j] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!fourCustomers[p][i][j].ContainsKey(k))
                                        fourCustomers[p][i][j][k] = new Dictionary<int, double>();

                                    if (!fourCustomers[p][i][j].ContainsKey(k))
                                        fourCustomers[p][i][j][k] = new Dictionary<int, double>();

                                    fourCustomers[p][i][j][k].Add(l, c[p][i][j][k][l]);
                                }
                                else if (i == j && k != i && l != i && k != l && (s[j] < s[k]) && (s[k] < s[l]))// 1 1 x x
                                {
                                    c[p][i][j][k].Add(l, cc[p, i] + cc[p, k] + cc[p, l]);
                                    double tval = (2 * tt[p, i]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[i] * vold[i]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 30;
                                    t[p][i][j][k].Add(l, tval);

                                    if (!threeCustomers.ContainsKey(p))
                                        threeCustomers[p] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                    if (!threeCustomers[p].ContainsKey(j))
                                        threeCustomers[p][j] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!threeCustomers[p][j][k].ContainsKey(l))
                                        threeCustomers[p][j][k] = new Dictionary<int, double>();

                                    threeCustomers[p][j][k].Add(l, c[p][i][j][k][l]);
                                }
                                else if (j == k && i != j && l != j && l != i && (s[i] < s[j]) && (s[j] < s[l])) // x 1 1 x
                                {
                                    c[p][i][j][k].Add(l, cc[p, i] + cc[p, k] + cc[p, l]);
                                    double tval = (2 * tt[p, i]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[i] * vold[i]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 30;
                                    t[p][i][j][k].Add(l, tval);

                                    if (!threeCustomers.ContainsKey(p))
                                        threeCustomers[p] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                    if (!threeCustomers[p].ContainsKey(i))
                                        threeCustomers[p][i] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!threeCustomers[p][i][j].ContainsKey(l))
                                        threeCustomers[p][i][j] = new Dictionary<int, double>();
                                    threeCustomers[p][i][j].Add(l, c[p][i][j][k][l]);
                                }
                                else if (k == l && i != k && j != k && i != j && (s[i] < s[j]) && (s[j] < s[l])) // x x 1 1
                                {
                                    c[p][i][j][k].Add(l, cc[p, i] + cc[p, j] + cc[p, k]);
                                    double tval = (2 * tt[p, i]) + (2 * tt[p, j]) + (2 * tt[p, k]) +
                                        (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + 30;
                                    t[p][i][j][k].Add(l, tval);

                                    if (!threeCustomers.ContainsKey(p))
                                        threeCustomers[p] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                    if (!threeCustomers[p].ContainsKey(i))
                                        threeCustomers[p][i] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!threeCustomers[p][i][j].ContainsKey(l))
                                        threeCustomers[p][i][j] = new Dictionary<int, double>();
                                    threeCustomers[p][i][j].Add(l, c[p][i][j][k][l]);
                                }
                                else if (i == j && j == k && k != l && (s[k] < s[l])) // 1 1 1 x
                                {
                                    c[p][i][j][k].Add(l, cc[p, k] + cc[p, l]);
                                    double tval = (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 20;
                                    t[p][i][j][k].Add(l, tval);
                                    if (!twoCustomers.ContainsKey(p))
                                        twoCustomers[p] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!twoCustomers[p].ContainsKey(k))
                                        twoCustomers[p][k] = new Dictionary<int, double>();
                                    twoCustomers[p][k].Add(l, c[p][i][j][k][l]);
                                }
                                else if (j == k && k == l && i != j && (s[i] < s[j])) // x 1 1 1
                                {
                                    c[p][i][j][k].Add(l, cc[p, i] + cc[p, j]);
                                    double tval = (2 * tt[p, i]) + (2 * tt[p, j]) +
                                        (cfr[j] * vold[j]) + (cfr[j] * vold[j]) + 20;
                                    t[p][i][j][k].Add(l, tval);
                                    if (!twoCustomers.ContainsKey(p))
                                        twoCustomers[p] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!twoCustomers[p].ContainsKey(i))
                                        twoCustomers[p][i] = new Dictionary<int, double>();
                                    twoCustomers[p][i].Add(j, c[p][i][j][k][l]);
                                }
                                else if (i == j && k == l && j != k && (s[j] < s[k])) // 1 1 0 0
                                {
                                    c[p][i][j][k].Add(l, cc[p, j] + cc[p, k]);
                                    double tval = (2 * tt[p, j]) + (2 * tt[p, k]) +
                                        (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + 20;
                                    t[p][i][j][k].Add(l, tval);
                                    if (!twoCustomers.ContainsKey(p))
                                        twoCustomers[p] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!twoCustomers[p].ContainsKey(j))
                                        twoCustomers[p][j] = new Dictionary<int, double>();
                                    twoCustomers[p][j].Add(k, c[p][i][j][k][l]);
                                }
                                else if (i == j && j == k && k == l) // 1 1 1 1 
                                {
                                    if (!c[p][i][j][k].ContainsKey(l))
                                    c[p][i][j][k].Add(l, cc[p, i]);
                                    double tval = (2 * tt[p, i]) + (cfr[i] * vold[i]) + 10;
                                    t[p][i][j][k].Add(l, tval);
                                    if (!oneCustomers.ContainsKey(p))
                                        oneCustomers[p] = new Dictionary<int, double>();
                                    oneCustomers[p].Add(i, c[p][i][j][k][l]);
                                }
                            }
                        }
                    }
                }
            }

            double totalCost = 0;
            List<int> customersAlreadyVisited = new List<int>();
            List<string> routes = new List<string>();
            fourCustomers.OrderBy(x => x.Value.Values.OrderBy(y => y.Values.OrderBy(z => z.Values.OrderBy(w => w.Values.OrderBy(s => s)))));
            foreach (var dictPair1 in fourCustomers)
            {
                foreach(var dictPair2 in dictPair1.Value)
                {
                    foreach (var dictPair3 in dictPair2.Value)
                    {
                        foreach (var dictPair4 in dictPair3.Value)
                        {
                            foreach (var dictPair5 in dictPair4.Value)
                            { 
                                if (!customersAlreadyVisited.Any(x => x == dictPair2.Key) &&
                                    !customersAlreadyVisited.Any(x => x == dictPair3.Key) &&
                                    !customersAlreadyVisited.Any(x => x == dictPair4.Key) &&
                                    !customersAlreadyVisited.Any(x => x == dictPair5.Key))
                                {
                                    customersAlreadyVisited.Add(dictPair2.Key);
                                    customersAlreadyVisited.Add(dictPair3.Key);
                                    customersAlreadyVisited.Add(dictPair4.Key);
                                    customersAlreadyVisited.Add(dictPair5.Key);
                                    routes.Add($"{dictPair1.Key} -> {dictPair2.Key} -> {dictPair3.Key} -> {dictPair4.Key} -> {dictPair5.Key}");
                                    totalCost += dictPair5.Value;
                                }
                            }
                        }
                    }
                }
            }

            threeCustomers.OrderBy(x => x.Value.Values.OrderBy(y => y.Values.OrderBy(z => z.Values.OrderBy(w => w))));
            foreach (var dictPair1 in threeCustomers)
            {
                foreach (var dictPair2 in dictPair1.Value)
                {
                    foreach (var dictPair3 in dictPair2.Value)
                    {
                        foreach (var dictPair4 in dictPair3.Value)
                        {
                            if (!customersAlreadyVisited.Any(x => x == dictPair2.Key) &&
                                !customersAlreadyVisited.Any(x => x == dictPair3.Key) &&
                                !customersAlreadyVisited.Any(x => x == dictPair4.Key))
                            {
                                customersAlreadyVisited.Add(dictPair2.Key);
                                customersAlreadyVisited.Add(dictPair3.Key);
                                customersAlreadyVisited.Add(dictPair4.Key);
                                routes.Add($"{dictPair1.Key} -> {dictPair2.Key} -> {dictPair3.Key} -> {dictPair4.Key}");
                                totalCost += dictPair4.Value;
                            }
                        }
                    }
                }
            }

            twoCustomers.OrderBy(x => x.Value.Values.OrderBy(y => y.Values.OrderBy(z => z)));
            foreach (var dictPair1 in twoCustomers)
            {
                foreach (var dictPair2 in dictPair1.Value)
                {
                    foreach (var dictPair3 in dictPair2.Value)
                    {
                        if (!customersAlreadyVisited.Any(x => x == dictPair2.Key) &&
                            !customersAlreadyVisited.Any(x => x == dictPair3.Key))
                        {
                            customersAlreadyVisited.Add(dictPair2.Key);
                            customersAlreadyVisited.Add(dictPair3.Key);
                            routes.Add($"{dictPair1.Key} -> {dictPair2.Key} -> {dictPair3.Key}");
                            totalCost += dictPair3.Value;
                        }
                    }
                }
            }

            oneCustomers.OrderBy(x => x.Value.Values.OrderBy(y => y));
            foreach (var dictPair1 in oneCustomers)
            {
                foreach (var dictPair2 in dictPair1.Value)
                {
                    if (!customersAlreadyVisited.Any(x => x == dictPair2.Key))
                    {
                        customersAlreadyVisited.Add(dictPair2.Key);
                        routes.Add($"{dictPair1.Key} -> {dictPair2.Key}");
                        totalCost += dictPair2.Value;
                    }
                }
            }
        }
    }
}
