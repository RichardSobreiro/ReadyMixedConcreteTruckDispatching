using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Heuristics
{
    class SimpleHeuristicGoogleMaps
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
                loadingPlace.MixerTrucks = mixerTrucks.Where(mt => mt.CODCENTCUS == loadingPlace.CODCENTCUS ||
                    (loadingPlaceSister != null && mt.CODCENTCUS == loadingPlaceSister.CODCENTCUS)).ToList();
            }

            foreach (Order order in orders)
            {
                order.TRIPS = deliveries.Where(d => d.CODPROGRAMACAO == order.CODPROGRAMACAO).ToList();
                order.LoadingPlaceCostGoogleMaps = double.MaxValue;
                foreach (LoadingPlace loadingPlace in loadingPlaces)
                {
                    double loadingPlaceCostGoogleMaps = 0;
                    foreach (Delivery delivery in order.TRIPS)
                    {
                        TrafficInfo.DirectionsResult directionsResult = trafficInfo.DirectionsResults.FirstOrDefault(dr =>
                            Math.Round(dr.OriginLatitude, 6) == loadingPlace.LATITUDE_FILIAL &&
                            Math.Round(dr.OriginLongitude, 6) == loadingPlace.LONGITUDE_FILIAL &&
                            (Math.Round(dr.DestinyLatitude, 8) >= (order.LATITUDE_OBRA - .00000002) ||
                                Math.Round(dr.DestinyLatitude, 8) <= (order.LATITUDE_OBRA + .00000002)) &&
                            (Math.Round(dr.DestinyLongitude, 8) >= (order.LONGITUDE_OBRA - .00000002) ||
                                Math.Round(dr.DestinyLongitude, 8) <= (order.LONGITUDE_OBRA + .00000002)) &&
                            dr.Hour == delivery.EndLoadingTime.Hour);
                        if(directionsResult == null)
                        {
                            directionsResult = trafficInfo.DirectionsResults.FirstOrDefault(dr =>
                            Math.Round(dr.OriginLatitude, 6) == loadingPlace.LATITUDE_FILIAL &&
                            Math.Round(dr.OriginLongitude, 6) == loadingPlace.LONGITUDE_FILIAL &&
                            (Math.Round(dr.DestinyLatitude, 8) >= (order.LATITUDE_OBRA - .00000002) ||
                                Math.Round(dr.DestinyLatitude, 8) <= (order.LATITUDE_OBRA + .00000002)) &&
                            (Math.Round(dr.DestinyLongitude, 8) >= (order.LONGITUDE_OBRA - .00000002) ||
                                Math.Round(dr.DestinyLongitude, 8) <= (order.LONGITUDE_OBRA + .00000002)));
                        }
                        LoadingPlace loadingPlaceInfo = loadingPlace.Clone();
                        loadingPlaceInfo.DISTANCE_GOOGLEMAPS = directionsResult.Distance;
                        loadingPlaceInfo.TRAVELTIME_GOOGLEMAPS = (int)directionsResult.TravelTime;
                        if (delivery.CUSVAR <= 0)
                            delivery.CUSVAR = DEFAULT_RMC_COST;
                        loadingPlaceInfo.CostGoogleMaps = (delivery.CUSVAR * delivery.VALVOLUMEPROG) + 
                            (loadingPlaceInfo.DISTANCE_GOOGLEMAPS * FIXED_L_PER_KM * 2 * DEFAULT_DIESEL_COST);
                        loadingPlaceCostGoogleMaps += loadingPlaceInfo.CostGoogleMaps;
                        delivery.LOADINGPLACES_INFO.Add(loadingPlaceInfo);
                    }
                    if (loadingPlaceCostGoogleMaps < order.LoadingPlaceCostGoogleMaps && loadingPlace.MixerTrucks.Count > 0)
                    {
                        order.CODCENTCUS = loadingPlace.CODCENTCUS;
                        for (int i = 0; i < order.TRIPS.Count; i++)
                            order.TRIPS[i].CODCENTCUSVIAGEM = loadingPlace.CODCENTCUS;
                        order.LoadingPlaceCostGoogleMaps = loadingPlaceCostGoogleMaps;
                    }
            }
            }
            List<Delivery> deliveryResults = new List<Delivery>();
            foreach (LoadingPlace loadingPlace in loadingPlaces)
            {
                LoadingPlace loadingPlaceSister = loadingPlaces.FirstOrDefault(
                    lps => lps.CODCENTCUS != loadingPlace.CODCENTCUS &&
                    lps.LATITUDE_FILIAL == loadingPlace.LATITUDE_FILIAL &&
                    lps.LONGITUDE_FILIAL == loadingPlace.LONGITUDE_FILIAL);
                List<Delivery> deliveriesOfThisLoadingPlace = orders.
                    Where(o => o.CODCENTCUS == loadingPlace.CODCENTCUS).
                    SelectMany(o => o.TRIPS).OrderBy(d => d.HORCHEGADAOBRA).ToList();
                loadingPlace.MixerTrucks = mixerTrucks.Where(mt => mt.CODCENTCUS == loadingPlace.CODCENTCUS ||
                    (loadingPlaceSister != null && mt.CODCENTCUS == loadingPlaceSister.CODCENTCUS)).ToList();
                for (int i = 0; i < deliveriesOfThisLoadingPlace.Count; i++)
                {
                    if (deliveryResults.Any(d => d.CODPROGVIAGEM == deliveriesOfThisLoadingPlace[i].CODPROGVIAGEM))
                        continue;
                    LoadingPlace loadingPlaceInfo = deliveriesOfThisLoadingPlace[i].LOADINGPLACES_INFO.FirstOrDefault(lpi =>
                        lpi.CODCENTCUS == loadingPlace.CODCENTCUS);
                    deliveriesOfThisLoadingPlace[i].ArrivalTimeAtConstruction = deliveriesOfThisLoadingPlace[i].HORCHEGADAOBRA;
                    deliveriesOfThisLoadingPlace[i].BeginLoadingTime =
                        deliveriesOfThisLoadingPlace[i].ArrivalTimeAtConstruction.
                            AddMinutes(-loadingPlaceInfo.TRAVELTIME_GOOGLEMAPS).
                            AddMinutes(-FIXED_LOADING_TIME);
                    deliveriesOfThisLoadingPlace[i].EndLoadingTime =
                        deliveriesOfThisLoadingPlace[i].BeginLoadingTime.AddMinutes(FIXED_LOADING_TIME);
                    deliveriesOfThisLoadingPlace[i].DepartureTimeAtConstruction =
                        deliveriesOfThisLoadingPlace[i].ArrivalTimeAtConstruction.
                            AddMinutes((int)(FIXED_CUSTOMER_FLOW_RATE * deliveriesOfThisLoadingPlace[i].VALVOLUMEPROG));
                    deliveriesOfThisLoadingPlace[i].ArrivalTimeAtLoadingPlace =
                        deliveriesOfThisLoadingPlace[i].DepartureTimeAtConstruction.AddMinutes(loadingPlaceInfo.TRAVELTIME_GOOGLEMAPS);

                    List<MixerTruck> mixerTrucksAvailableInUse = loadingPlace.MixerTrucks.Where(mt =>
                        mt.EndOfTheLastService != DateTime.MinValue &&
                        mt.EndOfTheLastService <= deliveriesOfThisLoadingPlace[i].BeginLoadingTime).ToList();
                    int codVeiculoSelected = 0;
                    if (mixerTrucksAvailableInUse.Count > 0)
                    {
                        TimeSpan idleTime = TimeSpan.MaxValue;
                        for (int k = 0; k < mixerTrucksAvailableInUse.Count; k++)
                        {
                            TimeSpan currentIdleTime = deliveriesOfThisLoadingPlace[i].BeginLoadingTime.
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
                                    mixerTruck.EndOfTheLastService.Subtract(deliveriesOfThisLoadingPlace[i].BeginLoadingTime);
                                if (currentLateness < lateness)
                                {
                                    lateness = currentLateness;
                                    codVeiculoSelected = mixerTruck.index;
                                }
                            }
                            deliveriesOfThisLoadingPlace[i].Lateness = (int)lateness.TotalMinutes;
                            deliveriesOfThisLoadingPlace[i].BeginLoadingTime =
                                deliveriesOfThisLoadingPlace[i].BeginLoadingTime.AddMinutes(lateness.TotalMinutes);
                            deliveriesOfThisLoadingPlace[i].EndLoadingTime =
                                deliveriesOfThisLoadingPlace[i].EndLoadingTime.AddMinutes(lateness.TotalMinutes);
                            deliveriesOfThisLoadingPlace[i].ArrivalTimeAtConstruction =
                                deliveriesOfThisLoadingPlace[i].ArrivalTimeAtConstruction.AddMinutes(lateness.TotalMinutes);
                            deliveriesOfThisLoadingPlace[i].DepartureTimeAtConstruction =
                                deliveriesOfThisLoadingPlace[i].DepartureTimeAtConstruction.AddMinutes(lateness.TotalMinutes);
                            deliveriesOfThisLoadingPlace[i].ArrivalTimeAtLoadingPlace =
                                deliveriesOfThisLoadingPlace[i].ArrivalTimeAtLoadingPlace.AddMinutes(lateness.TotalMinutes);
                        }
                    }
                    MixerTruck mixerTruckSelected = loadingPlace.MixerTrucks.FirstOrDefault(mt =>
                        mt.index == codVeiculoSelected);
                    if (mixerTruckSelected != null)
                    {
                        deliveriesOfThisLoadingPlace[i].CODVEICULO = codVeiculoSelected;
                        //deliveriesOfThisLoadingPlace[i].CODCENTCUSVIAGEM = loadingPlaceInfo.CODCENTCUS;
                        mixerTruckSelected.EndOfTheLastService = deliveriesOfThisLoadingPlace[i].ArrivalTimeAtLoadingPlace;
                        deliveryResults.Add(deliveriesOfThisLoadingPlace[i]);
                    }
                    else
                    {
                        Console.WriteLine($"No mixer truck available at loading place {loadingPlace.CODCENTCUS}");
                    }
                }
            }
            Result result = new Result();
            result.numberOfDeliveries = deliveryResults.Count;
            result.numberOfLoadingPlaces = loadingPlaces.Count;
            result.numberOfMixerTrucks = deliveryResults.GroupBy(d => d.CODVEICULO).Count();
            result.trips = new List<Result.ResultTrip>();
            foreach (Delivery delivery in deliveryResults)
            {
                LoadingPlace loadingPlaceInfo = delivery.LOADINGPLACES_INFO.FirstOrDefault(lpi =>
                    lpi.CODCENTCUS == delivery.CODCENTCUSVIAGEM);
                result.trips.Add(new Result.ResultTrip()
                {
                    OrderId = delivery.CODPROGRAMACAO,
                    Delivery = delivery.CODPROGVIAGEM,
                    MixerTruck = delivery.CODVEICULO,
                    LoadingBeginTime = (delivery.BeginLoadingTime.Hour * 60) + delivery.BeginLoadingTime.Minute,
                    ServiceTime = (delivery.ArrivalTimeAtConstruction.Hour * 60) + delivery.ArrivalTimeAtConstruction.Minute,
                    ReturnTime = (delivery.ArrivalTimeAtLoadingPlace.Hour * 60) + delivery.ArrivalTimeAtLoadingPlace.Minute,
                    LoadingPlant = delivery.CODCENTCUSVIAGEM,
                    Revenue = (int)((delivery.VLRVENDA * delivery.VALVOLUMEPROG) - loadingPlaceInfo.CostGoogleMaps),
                    BeginTimeWindow = (delivery.BeginLoadingTime.Hour * 60) + delivery.BeginLoadingTime.Minute,
                    EndTimeWindow = (delivery.ArrivalTimeAtLoadingPlace.Hour * 60) + delivery.ArrivalTimeAtLoadingPlace.Minute,
                    TravelTime = loadingPlaceInfo.TRAVELTIME_GOOGLEMAPS,
                    TravelCost = (int)loadingPlaceInfo.CostGoogleMaps,
                    DurationOfService = (int)(FIXED_CUSTOMER_FLOW_RATE * delivery.VALVOLUMEPROG),
                    IfDeliveryMustBeServed = 1,
                    CodDelivery = delivery.CODPROGVIAGEM,
                    CodOrder = delivery.CODPROGRAMACAO,
                    Lateness = delivery.Lateness
                });
            }
            result.objective = (int)(result.trips.Sum(rt => rt.Revenue) - (result.numberOfMixerTrucks * FIXED_MIXED_TRUCK_COST));
            string jsonString = JsonSerializer.Serialize(result);
            File.WriteAllText(folderPath + "\\ResultGoogleMapsSimpleHeuristic.json", jsonString);
        }
    }
}
