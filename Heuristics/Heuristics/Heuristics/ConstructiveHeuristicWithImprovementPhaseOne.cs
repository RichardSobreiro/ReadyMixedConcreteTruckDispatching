using CsvHelper;
using GeoCoordinatePortable;
using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Heuristics
{
    public class ConstructiveHeuristicWithImprovementPhaseOne
    {
        static Random random = new Random();

        public static void Execute(string folderPath, Result bestSolution)
        {
            double DEFAULT_DIESEL_COST = 3.5;
            double DEFAULT_RMC_COST = 150;
            double FIXED_MIXED_TRUCK_COST = 50;
            double FIXED_L_PER_KM = 27.5 / 100;
            int FIXED_LOADING_TIME = 8;

            #region Read Csv File
            List<CsvRow> records;
            using (var reader = new System.IO.StreamReader(folderPath + "Trips.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<CsvRow>().ToList();
            }
            #endregion
            int it = 0;
            while (it < 1000)
            {
                #region Get loading places, mixer trucks, orders and deliveries form csv
                int loadingPlaceIndex = 1;
                List<LoadingPlace> loadingPlaces = new List<LoadingPlace>();
                int mixerTruckIndex = 1;
                List<MixerTruck> mixerTrucks = new List<MixerTruck>();
                List<Order> orders = new List<Order>();
                List<Delivery> deliveries = new List<Delivery>();
                foreach (CsvRow csvRow in records)
                {
                    LoadingPlace loadingPlace = loadingPlaces.FirstOrDefault(lp => lp.CODCENTCUS == csvRow.CODCENTCUSVIAGEM);
                    if (loadingPlace == null)
                    {
                        loadingPlace = new LoadingPlace()
                        {
                            index = loadingPlaceIndex,
                            CODCENTCUS = csvRow.CODCENTCUSVIAGEM,
                            LATITUDE_FILIAL = csvRow.LATITUDE_FILIAL,
                            LONGITUDE_FILIAL = csvRow.LONGITUDE_FILIAL,
                            Coordinates = new GeoCoordinate(csvRow.LATITUDE_FILIAL, csvRow.LONGITUDE_FILIAL)
                        };
                        if (loadingPlace.CODCENTCUS == 17050 || loadingPlace.CODCENTCUS == 17250)
                        {
                            loadingPlace.LATITUDE_FILIAL = -23.689984;
                            loadingPlace.LONGITUDE_FILIAL = -46.602776;
                        }
                        if (loadingPlace.CODCENTCUS == 14050)
                        {
                            loadingPlace.LATITUDE_FILIAL = -23.152246;
                            loadingPlace.LONGITUDE_FILIAL = -45.801311;
                        }
                        loadingPlaces.Add(loadingPlace);
                        loadingPlaceIndex++;
                    }

                    MixerTruck mixerTruck = mixerTrucks.FirstOrDefault(mt => mt.CODVEICULO == csvRow.CODVEICULO);
                    if (mixerTruck == null)
                    {
                        mixerTruck = new MixerTruck()
                        {
                            index = mixerTruckIndex,
                            CODVEICULO = csvRow.CODVEICULO,
                            CODCENTCUS = csvRow.CODCENTCUS,
                            LATITUDE_FILIAL = csvRow.LATITUDE_FILIAL,
                            LONGITUDE_FILIAL = csvRow.LONGITUDE_FILIAL
                        };
                        mixerTrucks.Add(mixerTruck);
                        mixerTruckIndex++;
                    }

                    Order order = orders.FirstOrDefault(o => o.CODPROGRAMACAO == csvRow.CODPROGRAMACAO);
                    if (order == null)
                    {
                        order = new Order()
                        {
                            CODPROGRAMACAO = csvRow.CODPROGRAMACAO,
                            CODCENTCUS = csvRow.CODCENTCUSVIAGEM,
                            MEDIA_M3_DESCARGA = csvRow.MEDIA_M3_DESCARGA,
                            VALTOTALPROGRAMACAO = csvRow.VALTOTALPROGRAMACAO,
                            HORSAIDACENTRAL = csvRow.HORSAIDACENTRAL,
                            LATITUDE_OBRA = csvRow.LATITUDE_OBRA,
                            LONGITUDE_OBRA = csvRow.LONGITUDE_OBRA,
                            VLRVENDA = csvRow.VLRVENDA,
                            Coordinates = new GeoCoordinate(csvRow.LATITUDE_OBRA, csvRow.LONGITUDE_OBRA)
                        };
                        orders.Add(order);
                    }

                    Delivery delivery = deliveries.FirstOrDefault(d => d.CODPROGVIAGEM == csvRow.CODPROGVIAGEM);
                    if (delivery == null)
                    {
                        delivery = new Delivery()
                        {
                            HORCHEGADAOBRA = csvRow.HORCHEGADAOBRA,
                            CODPROGRAMACAO = csvRow.CODPROGRAMACAO,
                            CODPROGVIAGEM = csvRow.CODPROGVIAGEM,
                            CODCENTCUSVIAGEM = csvRow.CODCENTCUSVIAGEM,
                            VLRTOTALNF = csvRow.VLRTOTALNF,
                            VALVOLUMEPROG = csvRow.VALVOLUMEPROG,
                            CODTRACO = csvRow.CODTRACO,
                            CUSVAR = csvRow.CUSVAR,
                            VLRVENDA = csvRow.VLRVENDA,
                            LATITUDE_OBRA = csvRow.LATITUDE_OBRA,
                            LONGITUDE_OBRA = csvRow.LONGITUDE_OBRA,
                            MEDIA_M3_DESCARGA = csvRow.MEDIA_M3_DESCARGA
                        };
                        deliveries.Add(delivery);
                    }
                }
                #endregion

                #region Read traffic information per latitude and longitude
                TrafficInfo trafficInfo;
                using (StreamReader r = new StreamReader(folderPath + "DirectionsResultsStored.json"))
                {
                    string json = r.ReadToEnd();
                    trafficInfo = JsonSerializer.Deserialize<TrafficInfo>(json);
                }
                #endregion

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

                List<Delivery> deliveryResults = new List<Delivery>();

                #region Set Loading Place per Delivery
                foreach(Order order in orders)
                {
                    LoadingPlaceInfo loadingPlaceSmalestCost = order.LoadingPlaceInfos[0];
                    double randomValue = random.NextDouble();
                    if (randomValue >= 0.75 && randomValue <= 0.90 && order.LoadingPlaceInfos.Count > 2)
                    {
                        loadingPlaceSmalestCost = order.LoadingPlaceInfos[2];
                    }
                    else if (randomValue > 0.90 && order.LoadingPlaceInfos.Count > 4)
                    {
                        loadingPlaceSmalestCost = order.LoadingPlaceInfos[4];
                    }
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

                    double randomValue = random.NextDouble();

                    int codVeiculoSelected = 0;
                    if (mixerTrucksAvailableInUse.Count > 0 && randomValue >= 0.5)
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

                Result result = new Result();

                #region Compute Objective Function Cost
                List<int> codVeiculos = deliveryResults.GroupBy(dr => dr.CODVEICULO).Select(d => d.Key).ToList();
                Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
                int mixerTruckIndex2 = 1;
                foreach (Delivery delivery in deliveryResults)
                {
                    if (!keyValuePairs.ContainsKey(delivery.CODVEICULO))
                    {
                        keyValuePairs.Add(delivery.CODVEICULO, mixerTruckIndex2);
                        mixerTruckIndex2++;
                    }
                    delivery.CODVEICULO = keyValuePairs.GetValueOrDefault(delivery.CODVEICULO);
                }
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
                #endregion

                if(result.objective < bestSolution.objective)
                {
                    bestSolution = result;
                }
                it++;
            }

            WriteResults(bestSolution, folderPath);
        }

        static void WriteResults(Result bestSolution, string folderPath)
        {
            string jsonString = JsonSerializer.Serialize(bestSolution);
            File.WriteAllText(folderPath + "\\ResultConstructiveHeuristicWithImprovementPhaseOne.json", jsonString);
        }
    }
}
