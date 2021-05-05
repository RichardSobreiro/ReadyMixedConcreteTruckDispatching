using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Heuristics.ConstructiveHeuristics
{
    public class DeliveryByDeliveryAllocation
    {
        public static Delivery DeepCopy(Delivery self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<Delivery>(serialized);
        }
        public static void Execute(string folderPath)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

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

            List<Delivery> deliveries = new List<Delivery>();
            List<LoadingPlace> loadingPlaces = new List<LoadingPlace>();
            for (int j = 0; j < np; j++)
            {
                LoadingPlace loadingPlaceInfo = new LoadingPlace();
                loadingPlaceInfo.LoadingPlaceId = j;
                loadingPlaceInfo.CodLoadingPlace = codLoadingPlants[j];
                loadingPlaces.Add(loadingPlaceInfo);
            }
            for (int i = 0; i < nc; i++)
            {
                Delivery newDelivery = new Delivery();
                newDelivery.DeliveryId = i;
                newDelivery.CodDelivery = codDeliveries[i];
                newDelivery.CodOrder = codOrders[i];
                newDelivery.ServiceTime = s[i];
                newDelivery.CustomerFlowRate = cfr[i];
                newDelivery.Volume = vold[i];
                newDelivery.LoadingPlaceInfos = new List<LoadingPlaceInfo>();
                for (int j = 0; j < np; j++)
                {
                    LoadingPlaceInfo loadingPlaceInfo = new LoadingPlaceInfo();
                    loadingPlaceInfo.LoadingPlaceId = j;
                    loadingPlaceInfo.CodLoadingPlace = codLoadingPlants[j];
                    loadingPlaceInfo.TripDuration = tt[j, i];
                    loadingPlaceInfo.Cost = cc[j, i];
                    newDelivery.LoadingPlaceInfos.Add(loadingPlaceInfo);
                }
                deliveries.Add(newDelivery);
            }

            deliveries = deliveries.OrderBy(d => d.ServiceTime).ToList();
            List<Route> routes = new List<Route>();
            foreach (Delivery delivery in deliveries)
            {
                delivery.LoadingPlaceInfos = delivery.LoadingPlaceInfos.OrderBy(lp => lp.Cost).ToList();
                var cheapestLoadingPlace = delivery.LoadingPlaceInfos.FirstOrDefault();
                int maximalDeliveryLoadingTime = (delivery.ServiceTime + 15) -
                    cheapestLoadingPlace.TripDuration - 10;
                Route route = routes.FirstOrDefault(r => r.NextAvailableTime <= maximalDeliveryLoadingTime);
                if (route == null)
                {
                    route = new Route();
                    route.Deliveries = new List<Delivery>();
                    route.LoadingPlaceId = cheapestLoadingPlace.LoadingPlaceId;
                    route.CodLoadingPlace = cheapestLoadingPlace.CodLoadingPlace;
                    route.RouteString = $"Base [{route.LoadingPlaceId}] -> Custommer [{delivery.DeliveryId}]";
                    routes.Add(route);
                }
                else
                {
                    route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                }
                delivery.BaseLoadingPlaceId = cheapestLoadingPlace.LoadingPlaceId;
                delivery.CodLoadingPlace = cheapestLoadingPlace.CodLoadingPlace;
                if (!route.NextAvailableTime.HasValue ||
                    route.NextAvailableTime.HasValue && (route.NextAvailableTime <= (maximalDeliveryLoadingTime - 15)))
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                }
                else
                {
                    delivery.LoadingBeginTime = (route.NextAvailableTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = (route.NextAvailableTime) +
                        cheapestLoadingPlace.TripDuration + 10;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                }
                route.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                route.NumberOfCustomersInRoute++;
                route.TotalCost += cheapestLoadingPlace.Cost;
                route.Deliveries.Add(DeepCopy(delivery));
            }

            double minCost = 0;
            int trucksRouteCount = 1;
            foreach (var route in routes)
            {
                minCost += route.TotalCost;
                route.MixerTruck = trucksRouteCount;
                Console.WriteLine($"Truck Route [{trucksRouteCount}] : " + route.RouteString);
                trucksRouteCount++;
            }
            Console.WriteLine($"\n\nTotal Cost = {(routes.Count * 50) + minCost}");

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;
            Console.WriteLine($"\n\nTotal Elapsed Time: {Convert.ToInt32(stopwatchElapsed.TotalSeconds)}");
        }
    }
    public class Route
    {
        public int NumberOfCustomersInRoute { get; set; }
        public string RouteString { get; set; }
        public int? NextAvailableTime { get; set; }
        public int? MixerTruck { get; set; }
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
        public double TotalCost { get; set; }

        public List<Delivery> Deliveries { get; set; }
    };
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public int CodOrder { get; set; }
        public int CodDelivery { get; set; }
        public int ServiceTime { get; set; }
        public double CustomerFlowRate { get; set; }
        public double Volume { get; set; }
        public List<LoadingPlaceInfo> LoadingPlaceInfos { get; set; }

        public int? BaseLoadingPlaceId { get; set; }
        public int? CodLoadingPlace { get; set; }
        public int? LoadingBeginTime { get; set; }
        public int? BeginServiceTime { get; set; }
        public int? EndServiceTime { get; set; }
        public int? ArrivaTimeAtPlant { get; set; }
    }
    public class LoadingPlaceInfo
    {
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
        public int TripDuration { get; set; }
        public double Cost { get; set; }
    }
    public class LoadingPlace
    {
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
    }
}
