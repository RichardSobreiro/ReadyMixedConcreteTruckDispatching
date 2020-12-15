using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Heuristics
{
    public class NoTruckLimitationHeuristicGoogleMaps
    {
        public static void Execute(string folderPath, List<LoadingPlace> loadingPlaces, List<MixerTruck> mixerTrucks,
            List<Order> orders, List<Delivery> deliveries, TrafficInfo trafficInfo,
            double DEFAULT_DIESEL_COST, double DEFAULT_RMC_COST, double FIXED_MIXED_TRUCK_COST,
            double FIXED_MIXED_TRUCK_CAPACIT_M3, double FIXED_L_PER_KM, int FIXED_LOADING_TIME,
            int FIXED_CUSTOMER_FLOW_RATE)
        {
            foreach (LoadingPlace loadingPlace in loadingPlaces)
            {
                LoadingPlace loadingPlaceSister = loadingPlaces.FirstOrDefault(
                lps => lps.CODCENTCUS != loadingPlace.CODCENTCUS &&
                lps.LATITUDE_FILIAL == loadingPlace.LATITUDE_FILIAL &&
                lps.LONGITUDE_FILIAL == loadingPlace.LONGITUDE_FILIAL);
                if (loadingPlaceSister != null)
                    loadingPlace.CODCENTCUSSISTER = loadingPlaceSister.CODCENTCUS;
                loadingPlace.MixerTrucks = mixerTrucks.Where(mt => mt.CODCENTCUS == loadingPlace.CODCENTCUS ||
                    (loadingPlaceSister != null && mt.CODCENTCUS == loadingPlaceSister.CODCENTCUS)).ToList();
            }
            int maxLoadingPlaceIndex = loadingPlaces
                .Where(lp => lp.MixerTrucks.Count > 0)
                .Select(lp => lp.MixerTrucks.OrderByDescending(mt => mt.index).First())
                .ToList().FirstOrDefault().index;
            //foreach (LoadingPlace loadingPlace in loadingPlaces)
            //{
            //    if (loadingPlace.MixerTrucks.Count > 0)
            //    {
            //        List<MixerTruck> mixers = new List<MixerTruck>();
            //        for(int i = 0; i < 200; i++)
            //        {
            //            MixerTruck newMixerTruck = loadingPlace.MixerTrucks[0].Clone();
            //            newMixerTruck.EndOfTheLastService = DateTime.MinValue;
            //            newMixerTruck.index += maxLoadingPlaceIndex + 1 + i;
            //            maxLoadingPlaceIndex += i + 1;
            //            mixers.Add(newMixerTruck);
            //        }
            //        loadingPlace.MixerTrucks.AddRange(mixers);
            //    }
            //}

            foreach (Order order in orders)
            {
                order.TRIPS = deliveries.Where(d => d.CODPROGRAMACAO == order.CODPROGRAMACAO).ToList();
                foreach (LoadingPlace loadingPlace in loadingPlaces)
                {
                    if(order.LoadingPlaceInfos.Any(lpi => lpi.CODCENTCUS == loadingPlace.CODCENTCUS))
                    {
                        continue;
                    }
                    TrafficInfo.DirectionsResult directionsResult = trafficInfo.DirectionsResults.FirstOrDefault(dr =>
                        Math.Round(dr.OriginLatitude, 6) == loadingPlace.LATITUDE_FILIAL &&
                        Math.Round(dr.OriginLongitude, 6) == loadingPlace.LONGITUDE_FILIAL &&
                        (Math.Round(dr.DestinyLatitude, 8) == order.LATITUDE_OBRA) &&
                        (Math.Round(dr.DestinyLongitude, 8) == order.LONGITUDE_OBRA));
                    LoadingPlaceInfo loadingPlaceInfo = loadingPlace.CreateInfo();
                    foreach (Delivery delivery in order.TRIPS)
                    {
                        TrafficInfo.DirectionsResult directionsResultDelivery = trafficInfo.DirectionsResults.FirstOrDefault(dr =>
                            Math.Round(dr.OriginLatitude, 6) == loadingPlace.LATITUDE_FILIAL &&
                            Math.Round(dr.OriginLongitude, 6) == loadingPlace.LONGITUDE_FILIAL &&
                            (Math.Round(dr.DestinyLatitude, 8) == order.LATITUDE_OBRA) &&
                            (Math.Round(dr.DestinyLongitude, 8) == order.LONGITUDE_OBRA) &&
                            (dr.Hour == delivery.HORCHEGADAOBRA.Hour || dr.Hour == (delivery.HORCHEGADAOBRA.Hour - 1)));
                        if (directionsResultDelivery == null)
                        {
                            directionsResultDelivery = directionsResult;
                        }
                        loadingPlaceInfo.Distance = directionsResultDelivery.Distance;
                        loadingPlaceInfo.TravelTime = (int)directionsResultDelivery.TravelTime;
                        if (delivery.CUSVAR <= 0)
                            delivery.CUSVAR = DEFAULT_RMC_COST;
                        loadingPlaceInfo.Cost += ((delivery.CUSVAR * delivery.VALVOLUMEPROG) +
                            (loadingPlaceInfo.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
                    }
                    order.LoadingPlaceInfos.Add(loadingPlaceInfo);
                }
                order.LoadingPlaceInfos = order.LoadingPlaceInfos.OrderBy(lpi => lpi.Cost).ToList();
            }

            List<Delivery> deliveryResults = new List<Delivery>();

            orders = orders.OrderBy(o => o.HORSAIDACENTRAL).ToList();
            foreach (Order order in orders)
            {
                bool orderCouldBeServed = false;
                int deliveryNotServerCod = 0;
                foreach (LoadingPlaceInfo loadingPlaceInfo in order.LoadingPlaceInfos)
                {
                    LoadingPlace loadingPlace = loadingPlaces.FirstOrDefault(lp => lp.CODCENTCUS == loadingPlaceInfo.CODCENTCUS &&
                        lp.MixerTrucks.Count > 0);
                    if (loadingPlace != null)
                    {
                        foreach (Delivery delivery in order.TRIPS)
                        {
                            if (DetermineMixerTruck(delivery, loadingPlace, loadingPlaceInfo, FIXED_LOADING_TIME,
                                order.MEDIA_M3_DESCARGA, FIXED_L_PER_KM, DEFAULT_DIESEL_COST))
                            {
                                orderCouldBeServed = true;
                            }
                            else
                            {
                                orderCouldBeServed = false;
                                deliveryNotServerCod = delivery.CODPROGVIAGEM;
                                break;
                            }
                        }
                    }
                    if (orderCouldBeServed)
                    {
                        order.CODCENTCUS = loadingPlaceInfo.CODCENTCUS;
                        break;
                    }
                    else
                    {
                        order.CODCENTCUS = 0;
                    }
                }
                if (orderCouldBeServed == false)
                {
                    Console.WriteLine($"Delivery {deliveryNotServerCod} of the Order {order.CODPROGRAMACAO} could not be served...");
                    deliveryNotServerCod = 0;
                }
                deliveryResults.AddRange(order.TRIPS);
            }

            WriteResults(deliveryResults, loadingPlaces, FIXED_CUSTOMER_FLOW_RATE,
                FIXED_MIXED_TRUCK_COST, folderPath);
        }

        static bool DetermineMixerTruck(Delivery delivery, LoadingPlace loadingPlace,
            LoadingPlaceInfo loadingPlaceInfo, double FIXED_LOADING_TIME,
            double FIXED_CUSTOMER_FLOW_RATE, double FIXED_L_PER_KM,
            double DEFAULT_DIESEL_COST)
        {
            delivery.Cost = ((delivery.CUSVAR * delivery.VALVOLUMEPROG) +
                (loadingPlaceInfo.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
            delivery.TravelTime = loadingPlaceInfo.TravelTime;
            delivery.ArrivalTimeAtConstruction = delivery.HORCHEGADAOBRA;
            delivery.BeginLoadingTime =
                delivery.ArrivalTimeAtConstruction.
                    AddMinutes(-loadingPlaceInfo.TravelTime).
                    AddMinutes(-FIXED_LOADING_TIME);
            delivery.EndLoadingTime =
                delivery.BeginLoadingTime.AddMinutes(FIXED_LOADING_TIME);
            delivery.DepartureTimeAtConstruction =
                delivery.ArrivalTimeAtConstruction.
                    AddMinutes((int)(FIXED_CUSTOMER_FLOW_RATE * delivery.VALVOLUMEPROG));
            delivery.ArrivalTimeAtLoadingPlace =
                delivery.DepartureTimeAtConstruction.AddMinutes(loadingPlaceInfo.TravelTime);

            List<MixerTruck> mixerTrucksAvailableInUse = loadingPlace.MixerTrucks.Where(mt =>
                mt.EndOfTheLastService != DateTime.MinValue &&
                mt.EndOfTheLastService <= delivery.BeginLoadingTime).ToList();
            int codVeiculoSelected = 0;
            if (mixerTrucksAvailableInUse.Count > 0)
            {
                TimeSpan idleTime = TimeSpan.MaxValue;
                for (int k = 0; k < mixerTrucksAvailableInUse.Count; k++)
                {
                    TimeSpan currentIdleTime = delivery.BeginLoadingTime.
                        Subtract(mixerTrucksAvailableInUse[k].EndOfTheLastService);
                    if (currentIdleTime < idleTime)
                    {
                        idleTime = currentIdleTime;
                        codVeiculoSelected = mixerTrucksAvailableInUse[k].index;
                    }
                    currentIdleTime = TimeSpan.MaxValue;
                }
            }
            else
            {
                MixerTruck mixerTruckAvailableNotInUse = loadingPlace.MixerTrucks.FirstOrDefault(mt =>
                    mt.EndOfTheLastService == DateTime.MinValue);
                codVeiculoSelected = mixerTruckAvailableNotInUse != null ? mixerTruckAvailableNotInUse.index : 0;
                TimeSpan lateness = TimeSpan.MaxValue;
                if (codVeiculoSelected == 0)
                {
                    foreach (MixerTruck mixerTruck in loadingPlace.MixerTrucks)
                    {
                        TimeSpan currentLateness =
                            mixerTruck.EndOfTheLastService.Subtract(delivery.BeginLoadingTime);
                        if (currentLateness < lateness)
                        {
                            lateness = currentLateness;
                            codVeiculoSelected = mixerTruck.index;
                        }
                    }
                    if (lateness.TotalMinutes > 15)
                        return false;
                    delivery.Cost = ((delivery.CUSVAR * delivery.VALVOLUMEPROG) +
                        (loadingPlaceInfo.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
                    delivery.TravelTime = loadingPlaceInfo.TravelTime;
                    delivery.Lateness = (int)lateness.TotalMinutes;
                    delivery.BeginLoadingTime =
                        delivery.BeginLoadingTime.AddMinutes(lateness.TotalMinutes);
                    delivery.EndLoadingTime =
                        delivery.EndLoadingTime.AddMinutes(lateness.TotalMinutes);
                    delivery.ArrivalTimeAtConstruction =
                        delivery.ArrivalTimeAtConstruction.AddMinutes(lateness.TotalMinutes);
                    delivery.DepartureTimeAtConstruction =
                        delivery.DepartureTimeAtConstruction.AddMinutes(lateness.TotalMinutes);
                    delivery.ArrivalTimeAtLoadingPlace =
                        delivery.ArrivalTimeAtLoadingPlace.AddMinutes(lateness.TotalMinutes);
                }
            }
            MixerTruck mixerTruckSelected = loadingPlace.MixerTrucks.FirstOrDefault(mt =>
                mt.index == codVeiculoSelected);
            if (mixerTruckSelected != null)
            {
                delivery.CODVEICULO = codVeiculoSelected;
                delivery.CODCENTCUSVIAGEM = loadingPlaceInfo.CODCENTCUS;
                mixerTruckSelected.EndOfTheLastService = delivery.ArrivalTimeAtLoadingPlace;
                return true;
            }
            else
            {
                return false;
            }
        }

        static void WriteResults(List<Delivery> deliveryResults, List<LoadingPlace> loadingPlaces, double FIXED_CUSTOMER_FLOW_RATE,
            double FIXED_MIXED_TRUCK_COST, string folderPath)
        {
            List<int> codVeiculos = deliveryResults.GroupBy(dr => dr.CODVEICULO).Select(d => d.Key).ToList();
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            int mixerTruckIndex = 1;
            foreach (Delivery delivery in deliveryResults)
            {
                if(!keyValuePairs.ContainsKey(delivery.CODVEICULO))
                {
                    keyValuePairs.Add(delivery.CODVEICULO, mixerTruckIndex);
                    mixerTruckIndex++;
                }
                delivery.CODVEICULO = keyValuePairs.GetValueOrDefault(delivery.CODVEICULO);
            }
            Result result = new Result();
            result.numberOfDeliveries = deliveryResults.Count;
            result.numberOfLoadingPlaces = loadingPlaces.Count;
            result.numberOfMixerTrucks = deliveryResults.GroupBy(d => d.CODVEICULO).Count();
            result.trips = new List<Result.ResultTrip>();
            foreach (Delivery delivery in deliveryResults)
            {
                result.trips.Add(new Result.ResultTrip()
                {
                    OrderId = delivery.CODPROGRAMACAO,
                    Delivery = delivery.CODPROGVIAGEM,
                    MixerTruck = delivery.CODVEICULO,
                    LoadingBeginTime = (delivery.BeginLoadingTime.Hour * 60) + delivery.BeginLoadingTime.Minute,
                    ServiceTime = ((delivery.ArrivalTimeAtConstruction.Day - delivery.BeginLoadingTime.Day) * 24 * 60) + 
                        (delivery.ArrivalTimeAtConstruction.Hour * 60) + delivery.ArrivalTimeAtConstruction.Minute,
                    ReturnTime = ((delivery.ArrivalTimeAtLoadingPlace.Day - delivery.BeginLoadingTime.Day) * 24 * 60) +
                        (delivery.ArrivalTimeAtLoadingPlace.Hour * 60) + delivery.ArrivalTimeAtLoadingPlace.Minute,
                    LoadingPlant = delivery.CODCENTCUSVIAGEM,
                    Revenue = (int)((delivery.VLRVENDA * delivery.VALVOLUMEPROG)),
                    BeginTimeWindow = (delivery.BeginLoadingTime.Hour * 60) + delivery.BeginLoadingTime.Minute,
                    EndTimeWindow = ((delivery.ArrivalTimeAtLoadingPlace.Day - delivery.BeginLoadingTime.Day) * 24 * 60) +
                        (delivery.ArrivalTimeAtLoadingPlace.Hour * 60) + delivery.ArrivalTimeAtLoadingPlace.Minute,
                    TravelTime = delivery.TravelTime,
                    TravelCost = (int)delivery.Cost,
                    DurationOfService = (int)(FIXED_CUSTOMER_FLOW_RATE * delivery.VALVOLUMEPROG),
                    IfDeliveryMustBeServed = 1,
                    CodDelivery = delivery.CODPROGVIAGEM,
                    CodOrder = delivery.CODPROGRAMACAO,
                    Lateness = delivery.Lateness
                });
            }
            result.objective = (int)(result.trips.Sum(rt => rt.TravelCost) + (result.numberOfMixerTrucks * FIXED_MIXED_TRUCK_COST));
            string jsonString = JsonSerializer.Serialize(result);
            File.WriteAllText(folderPath + "\\ResultNoTruckLimitationHeuristic.json", jsonString);
        }
    }
}
