using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Heuristics.DynamicAlgorithms.IndexedRoutes
{
    public static class FourCustomers
    {
        public class Route 
        {
            public int NumberOfCustomersInRoute { get; set; }
            public string RouteString { get; set; }
            public int? Compare1 { get; set; }
            public int? Compare2 { get; set; }
            public int? Compare3 { get; set; }
            public int? Compare4 { get; set; }

            public int? MixerTruck { get; set; }
            public int LoadingPlaceId { get; set; }
            public int Delivery1 { get; set; }
            public int Delivery2 { get; set; }
            public int Delivery3 { get; set; }
            public int Delivery4 { get; set; }
            public double RouteTotalCost { get; set; }
            public double RouteTotalTime { get; set; }
            public double cfr1 { get; set; }
            public double cfr2 { get; set; }
            public double cfr3 { get; set; }
            public double cfr4 { get; set; }
            public double s1 { get; set; }
            public double s2 { get; set; }
            public double s3 { get; set; }
            public double s4 { get; set; }
            public double cs1 { get; set; }
            public double cs2 { get; set; }
            public double cs3 { get; set; }
            public double cs4 { get; set; }
            public double TravelTime1 { get; set; }
            public double TravelTime2 { get; set; }
            public double TravelTime3 { get; set; }
            public double TravelTime4 { get; set; }
            public double Cost1 { get; set; }
            public double Cost2 { get; set; }
            public double Cost3 { get; set; }
            public double Cost4 { get; set; }
            public double CodLoadingPlace { get; set; }
            public double CodOrder1 { get; set; }
            public double CodOrder2 { get; set; }
            public double CodOrder3 { get; set; }
            public double CodOrder4 { get; set; }
            public double CodDelivery1 { get; set; }
            public double CodDelivery2 { get; set; }
            public double CodDelivery3 { get; set; }
            public double CodDelivery4 { get; set; }
        };

        public static void Execute(string folderPath)
        {
            #region GetInputParameters
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
                if (lineCounter >= 4 && lineCounter < (4 + np))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    var values = val.Split(',');
                    int nvtt = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            tt[nptt, nvtt] = int.Parse(value);
                            nvtt++;
                        }
                    }
                    nptt++;
                }
                else if (lineCounter >= (4 + np + 2) && lineCounter < (4 + np + 2 + np))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    var values = val.Split(',');
                    int nvcc = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            cc[npcc, nvcc] = int.Parse(value);
                            nvcc++;
                        }
                    }
                    npcc++;
                }
                else if (lineCounter == (4 + np + 2 + np + 1))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    val = val.Replace("s", "");
                    var values = val.Split(',');
                    int nvs = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            s[nvs] = int.Parse(value);
                            nvs++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 2))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    val = val.Replace("c", "");
                    val = val.Replace("f", "");
                    val = val.Replace("r", "");
                    var values = val.Split(',');
                    int nvcfr = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            cfr[nvcfr] = int.Parse(value);
                            nvcfr++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 3))
                {
                    string val = line.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    val = val.Replace("v", "");
                    val = val.Replace("o", "");
                    val = val.Replace("l", "");
                    val = val.Replace("d", "");
                    var values = val.Split(',');
                    int nvvold = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            vold[nvvold] = int.Parse(value);
                            nvvold++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 4))
                {
                    int index = line.IndexOf("codLoadingPlants");
                    string cleanPath = (index < 0)
                        ? line
                        : line.Remove(index, "codLoadingPlants".Length);
                    string val = cleanPath.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    var values = val.Split(',');
                    int nvvold = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            codLoadingPlants[nvvold] = int.Parse(value);
                            nvvold++;
                        }
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 5))
                {
                    int index = line.IndexOf("codOrders");
                    string cleanPath = (index < 0)
                        ? line
                        : line.Remove(index, "codOrders".Length);
                    string val = cleanPath.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    var values = val.Split(',');
                    int nvcodOrders = 0;
                    foreach (var value in values)
                    {
                        codOrders[nvcodOrders] = int.Parse(value);
                        nvcodOrders++;
                    }
                }
                else if (lineCounter == (4 + np + 2 + np + 6))
                {
                    int index = line.IndexOf("codDeliveries");
                    string cleanPath = (index < 0) ? line : line.Remove(index, "codDeliveries".Length);
                    string val = cleanPath.Replace("[", "");
                    val = val.Replace("]", "");
                    val = val.Replace(" ", "");
                    val = val.Replace("=", "");
                    val = val.Replace(";", "");
                    var values = val.Split(',');
                    int nvcodDeliveries = 0;
                    foreach (var value in values)
                    {
                        if (!string.IsNullOrEmpty(value))
                        {
                            codDeliveries[nvcodDeliveries] = int.Parse(value);
                            nvcodDeliveries++;
                        }
                    }
                }
                lineCounter++;
            }
            #endregion

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

            List<Route> feasibleRoutes = new List<Route>();

            for (int p = 0; p < np; p++)
            {
                for (int i = 0; i < nc; i++)
                {
                    for (int j = 0; j < nc; j++)
                    {
                        for (int k = 0; k < nc; k++)
                        {
                            for (int l = 0; l < nc; l++)
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
                                    (s[i] < s[j]) && (s[j] < s[k]) && (s[k] < s[l]) &&
                                    (s[i] + ((cfr[i] * vold[i]) + tt[p, i]) <= (s[j] + 15) - (tt[p, j] + 10)) &&
                                    (s[j] + ((cfr[j] * vold[j]) + tt[p, j]) <= (s[k] + 15) - (tt[p, k] + 10)) &&
                                    (s[k] + ((cfr[k] * vold[k]) + tt[p, k]) <= (s[l] + 15) - (tt[p, l] + 10))
                                    )
                                {
                                    double routeCost = cc[p, i] + cc[p, j] + cc[p, k] + cc[p, l];
                                    c[p][i][j][k].Add(l, routeCost);
                                    double routeTotalTime = (2 * tt[p, i]) + (2 * tt[p, j]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 40;
                                    t[p][i][j][k].Add(l, routeTotalTime);

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
                                    if (!fourCustomers[p][i][j][k].ContainsKey(l))
                                    { 
                                        fourCustomers[p][i][j][k].Add(l, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 4,
                                            RouteString = $"Base [{p}] -> Custommer [{i}] -> Custommer [{j}] -> Custommer [{k}] -> Custommer [{l}]",
                                            Compare1 = i,
                                            Compare2 = j,
                                            Compare3 = k,
                                            Compare4 = l,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }

                                }
                                else if (i == j && k != i && l != i && k != l && (s[j] < s[k]) && (s[k] < s[l]) &&
                                    (s[j] + ((cfr[j] * vold[j]) + tt[p, j]) <= (s[k] + 15) - (tt[p, k] + 10)) &&
                                    (s[k] + ((cfr[k] * vold[k]) + tt[p, k]) <= (s[l] + 15) - (tt[p, l] + 10))
                                    )// 1 1 x x        
                                {
                                    double routeCost = cc[p, i] + cc[p, k] + cc[p, l];
                                    c[p][i][j][k].Add(l, cc[p, i] + cc[p, k] + cc[p, l]);
                                    double routeTotalTime = (2 * tt[p, i]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[i] * vold[i]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 30;
                                    t[p][i][j][k].Add(l, routeTotalTime);

                                    if (!threeCustomers.ContainsKey(p))
                                        threeCustomers[p] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                    if (!threeCustomers[p].ContainsKey(j))
                                        threeCustomers[p][j] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!threeCustomers[p][j].ContainsKey(k))
                                        threeCustomers[p][j][k] = new Dictionary<int, double>();
                                    if (!threeCustomers[p][j][k].ContainsKey(l))
                                    {
                                        threeCustomers[p][j][k].Add(l, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 3,
                                            RouteString = $"Base [{p}] -> Custommer [{j}] -> Custommer [{k}] -> Custommer [{l}]",
                                            Compare1 = null,
                                            Compare2 = j,
                                            Compare3 = k,
                                            Compare4 = l,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }
                                }
                                else if (j == k && i != j && l != j && l != i && (s[i] < s[j]) && (s[j] < s[l]) &&
                                    (s[i] + ((cfr[i] * vold[i]) + tt[p, i]) <= (s[j] + 15) - (tt[p, j] + 10)) &&
                                    (s[j] + ((cfr[j] * vold[j]) + tt[p, j]) <= (s[l] + 15) - (tt[p, l] + 10))
                                    ) // x 1 1 x
                                {
                                    double routeCost = cc[p, i] + cc[p, k] + cc[p, l];
                                    c[p][i][j][k].Add(l, routeCost);
                                    double routeTotalTime = (2 * tt[p, i]) + (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[i] * vold[i]) + (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 30;
                                    t[p][i][j][k].Add(l, routeTotalTime);

                                    if (!threeCustomers.ContainsKey(p))
                                        threeCustomers[p] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                    if (!threeCustomers[p].ContainsKey(i))
                                        threeCustomers[p][i] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!threeCustomers[p][i].ContainsKey(j))
                                        threeCustomers[p][i][j] = new Dictionary<int, double>();
                                    if (!threeCustomers[p][i][j].ContainsKey(l))
                                    {
                                        threeCustomers[p][i][j].Add(l, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 3,
                                            RouteString = $"Base [{p}] -> Custommer [{i}] -> Custommer [{j}] -> Custommer [{l}]",
                                            Compare1 = i,
                                            Compare2 = j,
                                            Compare3 = null,
                                            Compare4 = l,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }
                                }
                                else if (k == l && i != k && j != k && i != j && (s[i] < s[j]) && (s[j] < s[l]) &&
                                    (s[i] + ((cfr[i] * vold[i]) + tt[p, i]) <= (s[j] + 15) - (tt[p, j] + 10)) &&
                                    (s[j] + ((cfr[j] * vold[j]) + tt[p, j]) <= (s[l] + 15) - (tt[p, l] + 10))
                                    ) // x x 1 1
                                {
                                    double routeCost = cc[p, i] + cc[p, j] + cc[p, k];
                                    c[p][i][j][k].Add(l, routeCost);
                                    double routeTotalTime = (2 * tt[p, i]) + (2 * tt[p, j]) + (2 * tt[p, k]) +
                                        (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + 30;
                                    t[p][i][j][k].Add(l, routeTotalTime);

                                    if (!threeCustomers.ContainsKey(p))
                                        threeCustomers[p] = new Dictionary<int, Dictionary<int, Dictionary<int, double>>>();
                                    if (!threeCustomers[p].ContainsKey(i))
                                        threeCustomers[p][i] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!threeCustomers[p][i].ContainsKey(j))
                                        threeCustomers[p][i][j] = new Dictionary<int, double>();
                                    if (!threeCustomers[p][i][j].ContainsKey(l))
                                    {
                                        threeCustomers[p][i][j].Add(l, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 3,
                                            RouteString = $"Base [{p}] -> Custommer [{i}] -> Custommer [{j}] -> Custommer [{l}]",
                                            Compare1 = i,
                                            Compare2 = j,
                                            Compare3 = null,
                                            Compare4 = l,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }
                                }
                                else if (i == j && j == k && k != l && (s[k] < s[l]) &&
                                    (s[k] + ((cfr[k] * vold[k]) + tt[p, k]) <= (s[l] + 15) - (tt[p, l] + 10))
                                    ) // 1 1 1 x
                                {
                                    double routeCost = cc[p, k] + cc[p, l];
                                    c[p][i][j][k].Add(l, routeCost);
                                    double routeTotalTime = (2 * tt[p, k]) + (2 * tt[p, l]) +
                                        (cfr[k] * vold[k]) + (cfr[l] * vold[l]) + 20;
                                    t[p][i][j][k].Add(l, routeTotalTime);
                                    if (!twoCustomers.ContainsKey(p))
                                        twoCustomers[p] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!twoCustomers[p].ContainsKey(k))
                                        twoCustomers[p][k] = new Dictionary<int, double>();
                                    if (!twoCustomers[p][k].ContainsKey(l))
                                    {
                                        twoCustomers[p][k].Add(l, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 2,
                                            RouteString = $"Base [{p}] -> Custommer [{k}] -> Custommer [{l}]",
                                            Compare1 = null,
                                            Compare2 = null,
                                            Compare3 = k,
                                            Compare4 = l,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }
                                }
                                else if (j == k && k == l && i != j && (s[i] < s[j]) &&
                                    (s[i] + ((cfr[i] * vold[i]) + tt[p, i]) <= (s[j] + 15) - (tt[p, j] + 10))
                                    ) // x 1 1 1
                                {
                                    double routeCost = cc[p, i] + cc[p, j];
                                    c[p][i][j][k].Add(l, routeCost);
                                    double routeTotalTime = (2 * tt[p, i]) + (2 * tt[p, j]) +
                                        (cfr[i] * vold[i]) + (cfr[j] * vold[j]) + 20;
                                    t[p][i][j][k].Add(l, routeTotalTime);
                                    if (!twoCustomers.ContainsKey(p))
                                        twoCustomers[p] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!twoCustomers[p].ContainsKey(i))
                                        twoCustomers[p][i] = new Dictionary<int, double>();
                                    if (!twoCustomers[p][i].ContainsKey(j))
                                    {
                                        twoCustomers[p][i].Add(j, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 2,
                                            RouteString = $"Base [{p}] -> Custommer [{i}] -> Custommer [{j}]",
                                            Compare1 = i,
                                            Compare2 = j,
                                            Compare3 = null,
                                            Compare4 = null,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }
                                }
                                else if (i == j && k == l && j != k && (s[j] < s[k]) &&
                                    (s[j] + ((cfr[j] * vold[j]) + tt[p, j]) <= (s[k] + 15) - (tt[p, k] + 10))
                                    ) // 1 1 0 0
                                {
                                    double routeCost = cc[p, j] + cc[p, k];
                                    c[p][i][j][k].Add(l, routeCost);
                                    double routeTotalTime = (2 * tt[p, j]) + (2 * tt[p, k]) +
                                        (cfr[j] * vold[j]) + (cfr[k] * vold[k]) + 20;
                                    t[p][i][j][k].Add(l, routeTotalTime);
                                    if (!twoCustomers.ContainsKey(p))
                                        twoCustomers[p] = new Dictionary<int, Dictionary<int, double>>();
                                    if (!twoCustomers[p].ContainsKey(j))
                                        twoCustomers[p][j] = new Dictionary<int, double>();
                                    if (!twoCustomers[p][j].ContainsKey(k))
                                    {
                                        twoCustomers[p][j].Add(k, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 2,
                                            RouteString = $"Base [{p}] -> Custommer [{j}] -> Custommer [{k}]",
                                            Compare1 = null,
                                            Compare2 = j,
                                            Compare3 = k,
                                            Compare4 = null,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }
                                }
                                else if (i == j && j == k && k == l) // 1 1 1 1 
                                {
                                    double routeCost = cc[p, i];
                                    if (!c[p][i][j][k].ContainsKey(l))
                                        c[p][i][j][k].Add(l, cc[p, i]);
                                    double routeTotalTime = (2 * tt[p, i]) + (cfr[i] * vold[i]) + 10;
                                    t[p][i][j][k].Add(l, routeTotalTime);
                                    if (!oneCustomers.ContainsKey(p))
                                        oneCustomers[p] = new Dictionary<int, double>();
                                    if(!oneCustomers[p].ContainsKey(i))
                                    {
                                        oneCustomers[p].Add(i, c[p][i][j][k][l]);
                                        var newFeasibleRoute = new Route()
                                        {
                                            NumberOfCustomersInRoute = 1,
                                            RouteString = $"Base [{p}] -> Custommer [{i}]",
                                            Compare1 = i,
                                            Compare2 = null,
                                            Compare3 = null,
                                            Compare4 = null,
                                            MixerTruck = null,
                                            LoadingPlaceId = p,
                                            Delivery1 = i,
                                            Delivery2 = j,
                                            Delivery3 = k,
                                            Delivery4 = l,
                                            RouteTotalCost = routeCost,
                                            RouteTotalTime = routeTotalTime,
                                            cfr1 = cfr[i],
                                            cfr2 = cfr[j],
                                            cfr3 = cfr[k],
                                            cfr4 = cfr[l],
                                            s1 = s[i],
                                            s2 = s[j],
                                            s3 = s[k],
                                            s4 = s[l],
                                            cs1 = s[i],
                                            cs2 = (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]) <= s[j] ? s[j] : (s[i] + (vold[i] * cfr[i]) + tt[p, i] + 10 + tt[p, j]),
                                            cs3 = (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]) <= s[k] ? s[k] : (s[j] + (vold[j] * cfr[j]) + tt[p, j] + 10 + tt[p, k]),
                                            cs4 = (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]) <= s[l] ? s[l] : (s[k] + (vold[k] * cfr[k]) + tt[p, k] + 10 + tt[p, l]),
                                            TravelTime1 = tt[p, i],
                                            TravelTime2 = tt[p, j],
                                            TravelTime3 = tt[p, k],
                                            TravelTime4 = tt[p, l],
                                            Cost1 = cc[p, i],
                                            Cost2 = cc[p, j],
                                            Cost3 = cc[p, k],
                                            Cost4 = cc[p, l],
                                            CodLoadingPlace = codLoadingPlants[p],
                                            CodOrder1 = codOrders[i],
                                            CodOrder2 = codOrders[j],
                                            CodOrder3 = codOrders[k],
                                            CodOrder4 = codOrders[l],
                                            CodDelivery1 = codDeliveries[i],
                                            CodDelivery2 = codDeliveries[j],
                                            CodDelivery3 = codDeliveries[k],
                                            CodDelivery4 = codDeliveries[l]
                                        };
                                        feasibleRoutes.Add(newFeasibleRoute);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            double totalCost = 0;
            List<int> customersAlreadyVisited = new List<int>();
            List<string> customersSequencePerRoute = new List<string>();
            fourCustomers.OrderBy(x => x.Value.Values.OrderBy(y => y.Values.OrderBy(z => z.Values.OrderBy(w => w.Values.OrderBy(s => s)))));
            foreach (var dictPair1 in fourCustomers)
            {
                foreach (var dictPair2 in dictPair1.Value)
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
                                    customersSequencePerRoute.Add($"{dictPair1.Key} -> {dictPair2.Key} -> {dictPair3.Key} -> {dictPair4.Key} -> {dictPair5.Key}");
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
                                customersSequencePerRoute.Add($"{dictPair1.Key} -> {dictPair2.Key} -> {dictPair3.Key} -> {dictPair4.Key}");
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
                            customersSequencePerRoute.Add($"{dictPair1.Key} -> {dictPair2.Key} -> {dictPair3.Key}");
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
                        customersSequencePerRoute.Add($"{dictPair1.Key} -> {dictPair2.Key}");
                        totalCost += dictPair2.Value;
                    }
                }
            }

            int routeCount = 1;
            foreach (var route in customersSequencePerRoute)
            {
                Console.WriteLine($"Truck Route [{routeCount}] : " + route);
                routeCount++;
            }
            Console.WriteLine($"\n\nTotal Cost = {(customersSequencePerRoute.Count * 50) + totalCost}\n\n");

            double minCost = 0;
            List<int> customersVisited = new List<int>();
            var orderedFeasibleRoutes = feasibleRoutes.
                OrderByDescending(fr => fr.NumberOfCustomersInRoute).
                ThenBy(fr => fr.RouteTotalCost);
            var routes = new List<Route>();
            foreach(var route in orderedFeasibleRoutes)
            {
                if((!route.Compare1.HasValue || (route.Compare1.HasValue && !customersVisited.Any(c => c == route.Compare1))) &&
                   (!route.Compare2.HasValue || (route.Compare2.HasValue && !customersVisited.Any(c => c == route.Compare2))) &&
                   (!route.Compare3.HasValue || (route.Compare3.HasValue && !customersVisited.Any(c => c == route.Compare3))) &&
                   (!route.Compare4.HasValue || (route.Compare4.HasValue && !customersVisited.Any(c => c == route.Compare4)))
                   )
                {
                    routes.Add(route);
                    minCost += route.RouteTotalCost;
                    if(route.Compare1.HasValue)
                        customersVisited.Add(route.Delivery1);
                    if (route.Compare2.HasValue)
                        customersVisited.Add(route.Delivery2);
                    if (route.Compare3.HasValue)
                        customersVisited.Add(route.Delivery3);
                    if (route.Compare4.HasValue)
                        customersVisited.Add(route.Delivery4);
                }
            }
            int trucksRouteCount = 1;
            foreach(var route in routes)
            {
                route.MixerTruck = trucksRouteCount;
                Console.WriteLine($"Truck Route [{trucksRouteCount}] : " + route.RouteString);
                trucksRouteCount++;
            }
            Console.WriteLine($"\n\nTotal Cost = {(routes.Count * 50) + minCost}");
        }
    }
}
