using CsvHelper;
using GeoCoordinatePortable;
using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Heuristics
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            string instanceName = args[0];
            string folderPath = args[1] + instanceName;
            double probability = double.Parse(args[2]);
            int maxK = int.Parse(args[3]);
            //DynamicAlgorithms.IndexedRoutes.FourCustomers.Execute(folderPath);
            //Heuristics.ConstructiveHeuristics.DeliveryByDeliveryAllocation.Execute(folderPath);
            var stochasticDeliveryAcceptance = 
                new ConstructiveHeuristics.StochasticAlgorithms.StochasticDeliveryAcceptance(probability, maxK);
            stochasticDeliveryAcceptance.Execute(folderPath);
            //double DEFAULT_DIESEL_COST = 3.5;
            //double DEFAULT_RMC_COST = 150;
            //double FIXED_MIXED_TRUCK_COST = 50;
            //double FIXED_MIXED_TRUCK_CAPACIT_M3 = 10;
            //double FIXED_L_PER_KM = 27.5 / 100;
            //int FIXED_LOADING_TIME = 8;
            //int FIXED_CUSTOMER_FLOW_RATE = 3;

            //#region Read Csv File
            //List<CsvRow> records;
            //using (var reader = new System.IO.StreamReader(folderPath + "Trips.csv"))
            //using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            //{
            //    records = csv.GetRecords<CsvRow>().ToList();
            //}
            //#endregion

            //#region Get loading places, mixer trucks, orders and deliveries form csv
            //int loadingPlaceIndex = 1;
            //List<LoadingPlace> loadingPlaces = new List<LoadingPlace>();
            //int mixerTruckIndex = 1;
            //List<MixerTruck> mixerTrucks = new List<MixerTruck>();
            //List<Order> orders = new List<Order>();
            //List<Delivery> deliveries= new List<Delivery>();
            //foreach (CsvRow csvRow in records)
            //{
            //    LoadingPlace loadingPlace = loadingPlaces.FirstOrDefault(lp => lp.CODCENTCUS == csvRow.CODCENTCUSVIAGEM);
            //    if (loadingPlace == null)
            //    {
            //        loadingPlace = new LoadingPlace()
            //        {
            //            index = loadingPlaceIndex,
            //            CODCENTCUS = csvRow.CODCENTCUSVIAGEM,
            //            LATITUDE_FILIAL = csvRow.LATITUDE_FILIAL,
            //            LONGITUDE_FILIAL = csvRow.LONGITUDE_FILIAL,
            //            Coordinates = new GeoCoordinate(csvRow.LATITUDE_FILIAL, csvRow.LONGITUDE_FILIAL)
            //        };
            //        if(loadingPlace.CODCENTCUS == 17050 || loadingPlace.CODCENTCUS == 17250)
            //        {
            //            loadingPlace.LATITUDE_FILIAL = -23.689984;
            //            loadingPlace.LONGITUDE_FILIAL = -46.602776;
            //        }
            //        if(loadingPlace.CODCENTCUS == 14050)
            //        {
            //            loadingPlace.LATITUDE_FILIAL = -23.152246;
            //            loadingPlace.LONGITUDE_FILIAL = -45.801311;
            //        }
            //        loadingPlaces.Add(loadingPlace);
            //        loadingPlaceIndex++;
            //    }

            //    MixerTruck mixerTruck = mixerTrucks.FirstOrDefault(mt => mt.CODVEICULO == csvRow.CODVEICULO);
            //    if(mixerTruck == null)
            //    {
            //        mixerTruck = new MixerTruck()
            //        {
            //            index = mixerTruckIndex,
            //            CODVEICULO = csvRow.CODVEICULO,
            //            CODCENTCUS = csvRow.CODCENTCUS,
            //            LATITUDE_FILIAL = csvRow.LATITUDE_FILIAL,
            //            LONGITUDE_FILIAL = csvRow.LONGITUDE_FILIAL
            //        };
            //        mixerTrucks.Add(mixerTruck);
            //        mixerTruckIndex++;
            //    }

            //    Order order = orders.FirstOrDefault(o => o.CODPROGRAMACAO == csvRow.CODPROGRAMACAO);
            //    if(order == null)
            //    {
            //        order = new Order()
            //        {
            //            CODPROGRAMACAO = csvRow.CODPROGRAMACAO,
            //            CODCENTCUS = csvRow.CODCENTCUSVIAGEM,
            //            MEDIA_M3_DESCARGA = csvRow.MEDIA_M3_DESCARGA,
            //            VALTOTALPROGRAMACAO = csvRow.VALTOTALPROGRAMACAO,
            //            HORSAIDACENTRAL = csvRow.HORSAIDACENTRAL,
            //            LATITUDE_OBRA = csvRow.LATITUDE_OBRA,
            //            LONGITUDE_OBRA = csvRow.LONGITUDE_OBRA,
            //            VLRVENDA = csvRow.VLRVENDA,
            //            Coordinates = new GeoCoordinate(csvRow.LATITUDE_OBRA, csvRow.LONGITUDE_OBRA)
            //        };
            //        orders.Add(order);
            //    }

            //    Delivery delivery = deliveries.FirstOrDefault(d => d.CODPROGVIAGEM == csvRow.CODPROGVIAGEM);
            //    if(delivery == null)
            //    {
            //        delivery = new Delivery()
            //        {
            //            HORCHEGADAOBRA = csvRow.HORCHEGADAOBRA,
            //            CODPROGRAMACAO = csvRow.CODPROGRAMACAO,
            //            CODPROGVIAGEM = csvRow.CODPROGVIAGEM,
            //            CODCENTCUSVIAGEM = csvRow.CODCENTCUSVIAGEM,
            //            VLRTOTALNF = csvRow.VLRTOTALNF,
            //            VALVOLUMEPROG = csvRow.VALVOLUMEPROG,
            //            CODTRACO = csvRow.CODTRACO,
            //            CUSVAR = csvRow.CUSVAR,
            //            VLRVENDA = csvRow.VLRVENDA,
            //            LATITUDE_OBRA = csvRow.LATITUDE_OBRA,
            //            LONGITUDE_OBRA = csvRow.LONGITUDE_OBRA,
            //            MEDIA_M3_DESCARGA = csvRow.MEDIA_M3_DESCARGA
            //        };
            //        deliveries.Add(delivery);
            //    }
            //}
            //#endregion

            //#region Read traffic information per latitude and longitude
            //TrafficInfo trafficInfo;
            //using (StreamReader r = new StreamReader(folderPath + "DirectionsResultsStored.json"))
            //{
            //    string json = r.ReadToEnd();
            //    trafficInfo = JsonSerializer.Deserialize<TrafficInfo>(json);
            //}
            //#endregion

            ////SimpleHeuristicHaversine.Execute(folderPath, loadingPlaces, mixerTrucks,
            ////orders, deliveries, trafficInfo,
            ////DEFAULT_DIESEL_COST, DEFAULT_RMC_COST, FIXED_MIXED_TRUCK_COST,
            ////FIXED_MIXED_TRUCK_CAPACIT_M3, FIXED_L_PER_KM, FIXED_LOADING_TIME,
            ////FIXED_CUSTOMER_FLOW_RATE);

            ////SimpleHeuristicGoogleMaps.Execute(folderPath, loadingPlaces, mixerTrucks,
            ////    orders, deliveries, trafficInfo,
            ////    DEFAULT_DIESEL_COST, DEFAULT_RMC_COST, FIXED_MIXED_TRUCK_COST,
            ////    FIXED_MIXED_TRUCK_CAPACIT_M3, FIXED_L_PER_KM, FIXED_LOADING_TIME,
            ////    FIXED_CUSTOMER_FLOW_RATE);

            ////NoTruckLimitationHeuristicGoogleMaps.Execute(folderPath, loadingPlaces, mixerTrucks,
            ////    orders, deliveries, trafficInfo,
            ////    DEFAULT_DIESEL_COST, DEFAULT_RMC_COST, FIXED_MIXED_TRUCK_COST,
            ////    FIXED_MIXED_TRUCK_CAPACIT_M3, FIXED_L_PER_KM, FIXED_LOADING_TIME,
            ////    FIXED_CUSTOMER_FLOW_RATE);

            //Result result = DeliveryByDeliveryAllocationHeuristicGoogleMaps.Execute(folderPath, loadingPlaces, mixerTrucks,
            //    orders, deliveries, trafficInfo,
            //    DEFAULT_DIESEL_COST, DEFAULT_RMC_COST, FIXED_MIXED_TRUCK_COST,
            //    FIXED_MIXED_TRUCK_CAPACIT_M3, FIXED_L_PER_KM, FIXED_LOADING_TIME);

            ////ConstructiveHeuristicWithImprovementPhaseOne.Execute(folderPath, result);

            //watch.Stop();

            //TimeSpan timeSpan = watch.Elapsed;

            //Console.WriteLine("Time: {0}h {1}m {2}s {3}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }
    }
}
