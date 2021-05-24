using Heuristics.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Heuristics.ConstructiveHeuristics.IteratedLocalSearch.UndoOnlySomeRoutes
{
    public class UndoRoutes
    {
        private readonly double PROBABILITY;
        private readonly int MAX_K;
        private readonly int MAX_ILS_ITERATIONS;
        private readonly double UNDO_PERCENTANGE;
        private readonly int MAX_LOCAL_SEARCH_ITERATIONS;
        public UndoRoutes(double probability, int maxK, int _MAX_ILS_ITERATIONS,
            double _UNDO_PERCENTANGE, int _MAX_LOCAL_SEARCH_ITERATIONS)
        {
            PROBABILITY = probability;
            MAX_K = maxK;
            MAX_ILS_ITERATIONS = _MAX_ILS_ITERATIONS;
            UNDO_PERCENTANGE = _UNDO_PERCENTANGE;
            MAX_LOCAL_SEARCH_ITERATIONS = _MAX_LOCAL_SEARCH_ITERATIONS;
        }
        public string Execute(string folderPath)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            #region GetInputParameters
            int nc = 0;
            int np = 0;
            int nv = 0;

            //string[] lines = File.ReadAllLines(folderPath + "\\BianchessiReal.dat");
            string[] lines = File.ReadAllLines(folderPath);

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
                Route route = routes.FirstOrDefault(r => r.NextAvailableTime <= maximalDeliveryLoadingTime && 
                    r.LoadingPlaceId == cheapestLoadingPlace.LoadingPlaceId);
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
                if(!route.NextAvailableTime.HasValue)
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                }
                else if (route.NextAvailableTime.HasValue && 
                    (route.NextAvailableTime <= (maximalDeliveryLoadingTime - 15)))
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                else if (route.NextAvailableTime.HasValue && 
                    (route.NextAvailableTime <= maximalDeliveryLoadingTime))
                {

                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10 + 
                        (route.NextAvailableTime - (maximalDeliveryLoadingTime - 15));
                    delivery.BeginServiceTime = delivery.LoadingBeginTime +
                        cheapestLoadingPlace.TripDuration + 10;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                delivery.Delay = delivery.BeginServiceTime.Value - delivery.ServiceTime;
                route.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                route.NumberOfCustomersInRoute++;
                route.TotalCost += cheapestLoadingPlace.Cost;
                route.Deliveries.Add(Heuristics.Extensions.ObjectExtensions.DeepClone<Delivery>(delivery));
            }

            double totalCost = 0;
            int trucksRouteCount = 1;
            foreach (var route in routes)
            {
                totalCost += route.TotalCost;
                route.MixerTruck = trucksRouteCount;
                trucksRouteCount++;
            }
            totalCost = (routes.Count * 50) + totalCost;

            #region Local Search: Per Loading Place
            List<int> diferentLoadingPlaces = routes.Select(r => r.LoadingPlaceId).Distinct().ToList();
            List<Route> stochasticRoutes = new List<Route>();
            List<Task<List<Route>>> tasks = new List<Task<List<Route>>>();

            foreach (int loadingPlaceId in diferentLoadingPlaces)
            {
                var routesFromLp = routes.Where(r => r.LoadingPlaceId == loadingPlaceId).ToList();
                tasks.Add(Task.Factory.StartNew(() => StochasticRouteCreation(routesFromLp, loadingPlaceId, MAX_K)));
            }

            var results = Task.WhenAll(tasks);
            for (int ctr = 0; ctr <= results.Result.Length - 1; ctr++)
            {
                var result = results.Result[ctr];
                stochasticRoutes.AddRange(result);
            }

            double totalCostStochasticRoute = 0;
            int trucksRouteCountStochasticRoute = 1;
            foreach (var route in stochasticRoutes)
            {
                totalCostStochasticRoute += route.TotalCost;
                route.MixerTruck = trucksRouteCountStochasticRoute;
                foreach(var delivery in route.Deliveries)
                {
                    delivery.LoadingPlaceInfos.Clear();
                    for (int j = 0; j < np; j++)
                    {
                        LoadingPlaceInfo loadingPlaceInfo = new LoadingPlaceInfo();
                        loadingPlaceInfo.LoadingPlaceId = j;
                        loadingPlaceInfo.CodLoadingPlace = codLoadingPlants[j];
                        loadingPlaceInfo.TripDuration = tt[j, delivery.DeliveryId];
                        loadingPlaceInfo.Cost = cc[j, delivery.DeliveryId];
                        delivery.LoadingPlaceInfos.Add(loadingPlaceInfo);
                    }
                }
                trucksRouteCountStochasticRoute++;
            }
            totalCostStochasticRoute = (stochasticRoutes.Count * 50) + totalCostStochasticRoute;
            #endregion

            #region LOOP: Perturbation and Local Search
            int countILS = 0;
            while (countILS <= MAX_ILS_ITERATIONS)
            {
                List<Route> ilsRoute = PerturbationAndLocalSearch(stochasticRoutes);
                double totalCostILSRoute = 0;
                int trucksRouteCountILSRoute = 1;
                foreach (var route in ilsRoute)
                {
                    totalCostILSRoute += route.TotalCost;
                    route.MixerTruck = trucksRouteCountILSRoute;
                    trucksRouteCountILSRoute++;
                }
                totalCostILSRoute = (ilsRoute.Count * 50) + totalCostILSRoute;
                if(totalCostILSRoute < totalCostStochasticRoute)
                {
                    stochasticRoutes = ilsRoute;
                    totalCostStochasticRoute = totalCostILSRoute;
                }
                countILS++;
            }
            #endregion

            stopwatch.Stop();
            TimeSpan stopwatchElapsed = stopwatch.Elapsed;

            return $"{totalCostStochasticRoute} {Convert.ToInt32(stopwatchElapsed.TotalSeconds)}";
        }
        private List<Route> PerturbationAndLocalSearch(List<Route> routes)
        {
            List<int> diferentLoadingPlaces = routes.Select(r => r.LoadingPlaceId).Distinct().ToList();
            List<Route> stochasticRoutes = new List<Route>();
            List<Task<List<Route>>> tasks = new List<Task<List<Route>>>();

            foreach (int loadingPlaceId in diferentLoadingPlaces)
            {
                var routesFromLp = routes.Where(r => r.LoadingPlaceId == loadingPlaceId).ToList();
                tasks.Add(Task.Factory.StartNew(() => PertubationForLocalSearch(routesFromLp, loadingPlaceId, MAX_K)));
            }

            var results = Task.WhenAll(tasks);
            for (int ctr = 0; ctr <= results.Result.Length - 1; ctr++)
            {
                var result = results.Result[ctr];
                stochasticRoutes.AddRange(result);
            }

            double totalCostStochasticRoute = 0;
            int trucksRouteCountStochasticRoute = 1;
            foreach (var route in stochasticRoutes)
            {
                totalCostStochasticRoute += route.TotalCost;
                route.MixerTruck = trucksRouteCountStochasticRoute;
                trucksRouteCountStochasticRoute++;
            }
            totalCostStochasticRoute = (stochasticRoutes.Count * 50) + totalCostStochasticRoute;
            return stochasticRoutes;
        }
        private List<Route> PertubationForLocalSearch(List<Route> routes, int loadingPlaceId, int numberOfIterations)
        {
            List<Delivery> deliveries = new List<Delivery>();
            int numberOfTrucksUsed = routes.Count();
            Random random = new Random();
            int maxUndoRoutes = (int)(routes.Count() * UNDO_PERCENTANGE);
            int countUndo = 0;
            List<int> indexesToRemove = new List<int>();
            int index = 0;
            foreach (Route route in routes)
            {
                double p = random.NextDouble();
                if(p >= PROBABILITY && countUndo <= maxUndoRoutes)
                {
                    List<Delivery> newDeliveries = Heuristics.Extensions.ObjectExtensions.DeepClone<List<Delivery>>(route.Deliveries);
                    deliveries.AddRange(newDeliveries);
                    countUndo++;
                    indexesToRemove.Add(index);
                }
                index++;
            }
            foreach(int i in indexesToRemove)
            {
                routes.RemoveAt(i);
            }
            foreach (Delivery delivery in deliveries)
            {
                delivery.LoadingPlaceInfos.RemoveAll(lp => lp.LoadingPlaceId != loadingPlaceId);
            }
            int newNumberOfTrucksUsed = 0;
            int k = 0;
            while (k <= numberOfIterations)
            {
                List<Route> newRoutes = PartialStochasticRouteAcceptance(deliveries, routes);
                newNumberOfTrucksUsed = newRoutes.Count();
                if (newNumberOfTrucksUsed < numberOfTrucksUsed)
                {
                    routes = newRoutes;
                    numberOfTrucksUsed = newNumberOfTrucksUsed;
                }
                k++;
            }
            return routes;
        }
        private List<Route> PartialStochasticRouteAcceptance(List<Delivery> deliveries, List<Route> routes)
        {
            Random random = new Random();
            double routeAcceptanceProbability = 0;
            deliveries = deliveries.OrderBy(d => d.ServiceTime).ToList();
            foreach (Delivery delivery in deliveries)
            {
                delivery.LoadingPlaceInfos = delivery.LoadingPlaceInfos.OrderBy(lp => lp.Cost).ToList();
                var cheapestLoadingPlace = delivery.LoadingPlaceInfos.FirstOrDefault();
                int maximalDeliveryLoadingTime = (delivery.ServiceTime + 15) -
                    cheapestLoadingPlace.TripDuration - 10;
                Route route = routes.FirstOrDefault(r => r.NextAvailableTime <= maximalDeliveryLoadingTime &&
                        r.LoadingPlaceId == cheapestLoadingPlace.LoadingPlaceId);
                if (route == null)
                {
                    route = CreateNewRoute(cheapestLoadingPlace);
                    route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                    routes.Add(route);
                }
                else
                {
                    List<Route> copyRoutes = Heuristics.Extensions.ObjectExtensions.DeepClone<List<Route>>(routes);
                    while (routeAcceptanceProbability <= PROBABILITY)
                    {
                        routeAcceptanceProbability = random.NextDouble();
                        if (routeAcceptanceProbability > PROBABILITY)
                        {
                            break;
                        }
                        else
                        {
                            if (route != null)
                                copyRoutes.RemoveAll(r => r.NextAvailableTime == route.NextAvailableTime);
                            route = copyRoutes.FirstOrDefault(r => r.NextAvailableTime <= maximalDeliveryLoadingTime &&
                                        r.LoadingPlaceId == cheapestLoadingPlace.LoadingPlaceId);
                        }
                    }
                    if (route == null)
                    {
                        route = CreateNewRoute(cheapestLoadingPlace);
                        route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                        routes.Add(route);
                    }
                    else
                    {
                        route = routes.FirstOrDefault(r => r.NextAvailableTime == route.NextAvailableTime &&
                                    r.LoadingPlaceId == cheapestLoadingPlace.LoadingPlaceId);
                        route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                    }
                    routeAcceptanceProbability = 0;
                }
                delivery.BaseLoadingPlaceId = cheapestLoadingPlace.LoadingPlaceId;
                delivery.CodLoadingPlace = cheapestLoadingPlace.CodLoadingPlace;
                if (!route.NextAvailableTime.HasValue)
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                }
                else if (route.NextAvailableTime.HasValue &&
                    (route.NextAvailableTime <= (maximalDeliveryLoadingTime - 15)))
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                else if (route.NextAvailableTime.HasValue &&
                    (route.NextAvailableTime <= maximalDeliveryLoadingTime))
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10 +
                        (route.NextAvailableTime - (maximalDeliveryLoadingTime - 15));
                    delivery.BeginServiceTime = delivery.LoadingBeginTime +
                        cheapestLoadingPlace.TripDuration + 10;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                delivery.Delay = delivery.BeginServiceTime.Value - delivery.ServiceTime;
                route.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                route.NumberOfCustomersInRoute++;
                route.TotalCost += cheapestLoadingPlace.Cost;
                route.Deliveries.Add(Heuristics.Extensions.ObjectExtensions.DeepClone<Delivery>(delivery));
            }
            return routes;
        }
        #region Local Search
        private List<Route> StochasticRouteCreation(List<Route> routes, int loadingPlaceId, int numberOfIterations)
        {
            List<Delivery> deliveries = new List<Delivery>();
            int numberOfTrucksUsed = routes.Count();
            foreach(Route route in routes)
            {
                deliveries.AddRange(route.Deliveries);
            }
            foreach(Delivery delivery in deliveries)
            {
                delivery.LoadingPlaceInfos.RemoveAll(lp => lp.LoadingPlaceId != loadingPlaceId);
            }
            int newNumberOfTrucksUsed = 0;
            int k = 0;
            while(k <= numberOfIterations)
            {
                List<Route> newRoutes = StochasticRouteAcceptance(deliveries);
                newNumberOfTrucksUsed = newRoutes.Count();
                if(newNumberOfTrucksUsed < numberOfTrucksUsed)
                {
                    routes = newRoutes;
                    numberOfTrucksUsed = newNumberOfTrucksUsed;
                }
                k++;
            }
            return routes;
        }
        private List<Route> StochasticRouteAcceptance(List<Delivery> deliveries)
        {
            Random random = new Random();
            double routeAcceptanceProbability = 0;
            deliveries = deliveries.OrderBy(d => d.ServiceTime).ToList();
            List<Route> routes = new List<Route>();
            foreach (Delivery delivery in deliveries)
            {
                delivery.LoadingPlaceInfos = delivery.LoadingPlaceInfos.OrderBy(lp => lp.Cost).ToList();
                var cheapestLoadingPlace = delivery.LoadingPlaceInfos.FirstOrDefault();
                int maximalDeliveryLoadingTime = (delivery.ServiceTime + 15) -
                    cheapestLoadingPlace.TripDuration - 10;
                Route route = routes.FirstOrDefault(r => r.NextAvailableTime <= maximalDeliveryLoadingTime &&
                        r.LoadingPlaceId == cheapestLoadingPlace.LoadingPlaceId);
                if (route == null)
                {
                    route = CreateNewRoute(cheapestLoadingPlace);
                    route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                    routes.Add(route);
                }
                else
                {
                    List<Route> copyRoutes = Heuristics.Extensions.ObjectExtensions.DeepClone<List<Route>>(routes);
                    while (routeAcceptanceProbability <= PROBABILITY)
                    {
                        routeAcceptanceProbability = random.NextDouble();
                        if (routeAcceptanceProbability > PROBABILITY)
                        {
                            break;
                        }
                        else
                        {
                            if (route != null)
                                copyRoutes.RemoveAll(r => r.NextAvailableTime == route.NextAvailableTime);
                            route = copyRoutes.FirstOrDefault(r => r.NextAvailableTime <= maximalDeliveryLoadingTime &&
                                        r.LoadingPlaceId == cheapestLoadingPlace.LoadingPlaceId);
                        }
                    }
                    if (route == null)
                    {
                        route = CreateNewRoute(cheapestLoadingPlace);
                        route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                        routes.Add(route);
                    }
                    else
                    {
                        route = routes.FirstOrDefault(r => r.NextAvailableTime == route.NextAvailableTime &&
                                    r.LoadingPlaceId == cheapestLoadingPlace.LoadingPlaceId);
                        route.RouteString += $" -> Custommer [{delivery.DeliveryId}]";
                    }
                    routeAcceptanceProbability = 0;
                }
                delivery.BaseLoadingPlaceId = cheapestLoadingPlace.LoadingPlaceId;
                delivery.CodLoadingPlace = cheapestLoadingPlace.CodLoadingPlace;
                if (!route.NextAvailableTime.HasValue)
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                }
                else if (route.NextAvailableTime.HasValue &&
                    (route.NextAvailableTime <= (maximalDeliveryLoadingTime - 15)))
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10;
                    delivery.BeginServiceTime = delivery.ServiceTime;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                else if (route.NextAvailableTime.HasValue &&
                    (route.NextAvailableTime <= maximalDeliveryLoadingTime))
                {
                    delivery.LoadingBeginTime = (delivery.ServiceTime) -
                        cheapestLoadingPlace.TripDuration - 10 +
                        (route.NextAvailableTime - (maximalDeliveryLoadingTime - 15));
                    delivery.BeginServiceTime = delivery.LoadingBeginTime +
                        cheapestLoadingPlace.TripDuration + 10;
                    delivery.EndServiceTime = (int)(delivery.BeginServiceTime +
                        (delivery.Volume * delivery.CustomerFlowRate));
                    delivery.ArrivaTimeAtPlant = delivery.EndServiceTime +
                        cheapestLoadingPlace.TripDuration;
                    delivery.WaitingTimeBeforeLoading = delivery.LoadingBeginTime - route.NextAvailableTime;
                }
                delivery.Delay = delivery.BeginServiceTime.Value - delivery.ServiceTime;
                route.NextAvailableTime = delivery.ArrivaTimeAtPlant;
                route.NumberOfCustomersInRoute++;
                route.TotalCost += cheapestLoadingPlace.Cost;
                route.Deliveries.Add(Heuristics.Extensions.ObjectExtensions.DeepClone<Delivery>(delivery));
            }
            return routes;
        }
        private Route CreateNewRoute(LoadingPlaceInfo cheapestLoadingPlace)
        {
            Route route = new Route();
            route.Deliveries = new List<Delivery>();
            route.LoadingPlaceId = cheapestLoadingPlace.LoadingPlaceId;
            route.CodLoadingPlace = cheapestLoadingPlace.CodLoadingPlace;
            route.RouteString = $"Base [{route.LoadingPlaceId}]";
            return route;
        }
        #endregion
        static void WriteResults(List<Route> routes, string folderPath, string fileName, double totalCost)
        {
            List<Delivery> deliveryResults = new List<Delivery>();
            foreach (Route route in routes)
            {
                foreach(Delivery delivery in route.Deliveries)
                delivery.MixerTruck = route.MixerTruck.Value;
                deliveryResults.AddRange(route.Deliveries);
            }
            Result result = new Result();
            result.numberOfDeliveries = deliveryResults.Count;
            result.numberOfLoadingPlaces = 0;
            result.numberOfMixerTrucks = routes.Count();
            result.trips = new List<Result.ResultTrip>();
            foreach (Delivery delivery in deliveryResults)
            {
                result.trips.Add(new Result.ResultTrip()
                {
                    OrderId = delivery.CodOrder,
                    Delivery = delivery.CodDelivery,
                    MixerTruck = delivery.MixerTruck,
                    LoadingBeginTime = delivery.LoadingBeginTime.Value,
                    ServiceTime = delivery.BeginServiceTime.Value,
                    ReturnTime = delivery.ArrivaTimeAtPlant.Value,
                    LoadingPlant = delivery.LoadingPlaceInfos[0].CodLoadingPlace,
                    Revenue = 0,
                    BeginTimeWindow = delivery.ServiceTime,
                    EndTimeWindow = delivery.ServiceTime + 15,
                    TravelTime = delivery.LoadingPlaceInfos[0].TripDuration,
                    TravelCost = (int)delivery.LoadingPlaceInfos[0].Cost,
                    DurationOfService = (int)(delivery.EndServiceTime.Value - delivery.BeginServiceTime.Value),
                    IfDeliveryMustBeServed = 1,
                    CodDelivery = delivery.CodDelivery,
                    CodOrder = delivery.CodOrder,
                    Lateness = delivery.BeginServiceTime.Value - delivery.ServiceTime
                });
            }
            result.objective = (int)totalCost;
            string jsonString = JsonSerializer.Serialize(result);
            File.WriteAllText(folderPath + fileName, jsonString);
        }
    }
    [Serializable]
    public class Route
    {
        public int NumberOfCustomersInRoute { get; set; }
        public string RouteString { get; set; }
        public int? NextAvailableTime { get; set; }
        public int? MixerTruck { get; set; }
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
        public double TotalCost { get; set; }
        
        public int TotalWaitingTime 
        {
            get 
            {
                return Deliveries.Sum(d => d.WaitingTimeBeforeLoading.HasValue ? d.WaitingTimeBeforeLoading.Value : 0);
            }
        }

        public List<Delivery> Deliveries { get; set; }
    };
    [Serializable]
    public class Delivery
    {
        public int DeliveryId { get; set; }
        public int MixerTruck { get; set; }
        public int CodOrder { get; set; }
        public int CodDelivery { get; set; }
        public int ServiceTime { get; set; }
        public double CustomerFlowRate { get; set; }
        public double Volume { get; set; }
        public List<LoadingPlaceInfo> LoadingPlaceInfos { get; set; }

        public int? BaseLoadingPlaceId { get; set; }
        public int? CodLoadingPlace { get; set; }
        public int? WaitingTimeBeforeLoading { get; set; }
        public int Delay { get; set; }
        public int? LoadingBeginTime { get; set; }
        public int? BeginServiceTime { get; set; }
        public int? EndServiceTime { get; set; }
        public int? ArrivaTimeAtPlant { get; set; }
    }
    [Serializable]
    public class LoadingPlaceInfo
    {
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
        public int TripDuration { get; set; }
        public double Cost { get; set; }
    }
    [Serializable]
    public class LoadingPlace
    {
        public int LoadingPlaceId { get; set; }
        public int CodLoadingPlace { get; set; }
    }
}
