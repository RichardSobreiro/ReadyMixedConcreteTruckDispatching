using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Heuristics
{
    public class DeliveryByDeliveryAllocationHeuristicGoogleMaps
    {
        public static void Execute(string folderPath, List<LoadingPlace> loadingPlaces, List<MixerTruck> mixerTrucks,
            List<Order> orders, List<Delivery> deliveries, TrafficInfo trafficInfo,
            double DEFAULT_DIESEL_COST, double DEFAULT_RMC_COST, double FIXED_MIXED_TRUCK_COST,
            double FIXED_MIXED_TRUCK_CAPACIT_M3, double FIXED_L_PER_KM, int FIXED_LOADING_TIME)
        {
            #region Loading Place Sister
            foreach (LoadingPlace loadingPlace in loadingPlaces)
            {
                LoadingPlace loadingPlaceSister = loadingPlaces.FirstOrDefault(
                    lps => lps.CODCENTCUS != loadingPlace.CODCENTCUS &&
                    lps.LATITUDE_FILIAL == loadingPlace.LATITUDE_FILIAL &&
                    lps.LONGITUDE_FILIAL == loadingPlace.LONGITUDE_FILIAL);
                if (loadingPlaceSister != null)
                    loadingPlace.CODCENTCUSSISTER = loadingPlaceSister.CODCENTCUS;
            }
            #endregion

            #region Loading Place Infos Per Order
            foreach (Order order in orders)
            {
                order.TRIPS = deliveries.Where(d => d.CODPROGRAMACAO == order.CODPROGRAMACAO).ToList();
                foreach (LoadingPlace loadingPlace in loadingPlaces)
                {
                    if (order.LoadingPlaceInfos.Any(lpi => lpi.CODCENTCUS == loadingPlace.CODCENTCUS))
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
            #endregion

            #region Set Loading Place per Delivery
            List<Delivery> deliveryResults = new List<Delivery>();
            foreach(Order order in orders)
            {
                LoadingPlaceInfo loadingPlaceSmalestCost = order.LoadingPlaceInfos[0];
                foreach (Delivery delivery in order.TRIPS)
                {
                    delivery.CODCENTCUSVIAGEM = loadingPlaceSmalestCost.CODCENTCUS;
                    delivery.TravelTime = loadingPlaceSmalestCost.TravelTime;
                    delivery.Distance = loadingPlaceSmalestCost.Distance;
                    deliveryResults.Add(delivery);
                }
            }
            #endregion

            #region Set Mixer Truck Per Delivery
            deliveryResults = deliveryResults.OrderBy(d => d.HORCHEGADAOBRA).ToList();
            foreach (Delivery delivery in deliveryResults)
            {
                delivery.Cost = ((delivery.CUSVAR * delivery.VALVOLUMEPROG) +
                (delivery.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
                delivery.ArrivalTimeAtConstruction = delivery.HORCHEGADAOBRA;
                delivery.BeginLoadingTime =
                    delivery.ArrivalTimeAtConstruction.
                        AddMinutes(-delivery.TravelTime).
                        AddMinutes(-FIXED_LOADING_TIME);
                delivery.EndLoadingTime =
                    delivery.BeginLoadingTime.AddMinutes(FIXED_LOADING_TIME);
                delivery.DepartureTimeAtConstruction =
                    delivery.ArrivalTimeAtConstruction.
                        AddMinutes((int)(delivery.MEDIA_M3_DESCARGA * delivery.VALVOLUMEPROG));
                delivery.ArrivalTimeAtLoadingPlace =
                    delivery.DepartureTimeAtConstruction.AddMinutes(delivery.TravelTime);

                List<MixerTruck> mixerTrucksAvailableInUse = mixerTrucks.Where(mt =>
                    mt.EndOfTheLastService != DateTime.MinValue &&
                    mt.EndOfTheLastService <= delivery.BeginLoadingTime &&
                    mt.CODCENTCUS == delivery.CODCENTCUSVIAGEM).ToList();

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
                    MixerTruck mixerTruckAvailableNotInUse = mixerTrucks.FirstOrDefault(mt =>
                        mt.EndOfTheLastService == DateTime.MinValue);
                    codVeiculoSelected = mixerTruckAvailableNotInUse != null ? mixerTruckAvailableNotInUse.index : 0;
                    TimeSpan lateness = TimeSpan.MaxValue;
                    if (codVeiculoSelected == 0)
                    {
                        foreach (MixerTruck mixerTruck in mixerTrucks)
                        {
                            TimeSpan currentLateness =
                                mixerTruck.EndOfTheLastService.Subtract(delivery.BeginLoadingTime);
                            if (currentLateness < lateness)
                            {
                                lateness = currentLateness;
                                codVeiculoSelected = mixerTruck.index;
                            }
                        }
                        if (lateness.TotalMinutes <= 15)
                        {
                            delivery.Cost = ((delivery.CUSVAR * delivery.VALVOLUMEPROG) +
                                (delivery.Distance * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST));
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
                }
                MixerTruck mixerTruckSelected = mixerTrucks.FirstOrDefault(mt =>
                        mt.index == codVeiculoSelected);
                if (mixerTruckSelected != null)
                {
                    mixerTruckSelected.CODCENTCUS = delivery.CODCENTCUSVIAGEM;
                    delivery.CODVEICULO = codVeiculoSelected;
                    mixerTruckSelected.EndOfTheLastService = delivery.ArrivalTimeAtLoadingPlace;
                }
                else
                {
                    Console.WriteLine($"Delivery {delivery.CODCENTCUSVIAGEM} of the Order {delivery.CODPROGRAMACAO} could not be served...");
                }
            }
            #endregion

            WriteResults(deliveryResults, loadingPlaces,
                FIXED_MIXED_TRUCK_COST, folderPath);
        }

        static void WriteResults(List<Delivery> deliveryResults, List<LoadingPlace> loadingPlaces,
            double FIXED_MIXED_TRUCK_COST, string folderPath)
        {
            List<int> codVeiculos = deliveryResults.GroupBy(dr => dr.CODVEICULO).Select(d => d.Key).ToList();
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            int mixerTruckIndex = 1;
            foreach (Delivery delivery in deliveryResults)
            {
                if (!keyValuePairs.ContainsKey(delivery.CODVEICULO))
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
                    BeginTimeWindow = ((delivery.ArrivalTimeAtConstruction.Day - delivery.BeginLoadingTime.Day) * 24 * 60) +
                        (delivery.ArrivalTimeAtConstruction.Hour * 60) + delivery.ArrivalTimeAtConstruction.Minute,
                    EndTimeWindow = ((delivery.ArrivalTimeAtConstruction.Day - delivery.BeginLoadingTime.Day) * 24 * 60) +
                        (delivery.ArrivalTimeAtConstruction.Hour * 60) + delivery.ArrivalTimeAtConstruction.Minute,
                    TravelTime = delivery.TravelTime,
                    TravelCost = (int)delivery.Cost,
                    DurationOfService = (int)(delivery.MEDIA_M3_DESCARGA * delivery.VALVOLUMEPROG),
                    IfDeliveryMustBeServed = 1,
                    CodDelivery = delivery.CODPROGVIAGEM,
                    CodOrder = delivery.CODPROGRAMACAO,
                    Lateness = delivery.Lateness
                });
            }
            result.objective = (int)(result.trips.Sum(rt => rt.TravelCost) + (result.numberOfMixerTrucks * FIXED_MIXED_TRUCK_COST));
            string jsonString = JsonSerializer.Serialize(result);
            File.WriteAllText(folderPath + "\\ResultDeliveryByDeliveryAllocationHeuristic.json", jsonString);
        }
    }
}
