using CsvHelper;
using GeoCoordinatePortable;
using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
using System;
using System.Collections.Generic;
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
            string instanceName = "BH-10-01-2020\\";
            string folderPath = "C:\\Users\\Richard Sobreiro\\Google Drive\\Mestrado\\Dados\\" + instanceName;
            double DEFAULT_DIESEL_COST = 3.5;
            double DEFAULT_RMC_COST = 150;
            double FIXED_MIXED_TRUCK_COST = 50;
            double FIXED_MIXED_TRUCK_CAPACIT_M3 = 10;
            double FIXED_L_PER_KM = 27.5 / 100;
            int FIXED_LOADING_TIME = 8;
            int FIXED_CUSTOMER_FLOW_RATE = 3;

            #region Read Csv File
            List<CsvRow> records;
            using (var reader = new System.IO.StreamReader(folderPath + "Trips.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<CsvRow>().ToList();
            }
            #endregion

            #region Get loading places, mixer trucks, orders and deliveries form csv
            int loadingPlaceIndex = 1;
            List<LoadingPlace> loadingPlaces = new List<LoadingPlace>();
            int mixerTruckIndex = 1;
            List<MixerTruck> mixerTrucks = new List<MixerTruck>();
            List<Order> orders = new List<Order>();
            List<Delivery> deliveries= new List<Delivery>();
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
                    loadingPlaces.Add(loadingPlace);
                    loadingPlaceIndex++;
                }

                MixerTruck mixerTruck = mixerTrucks.FirstOrDefault(mt => mt.CODVEICULO == csvRow.CODVEICULO);
                if(mixerTruck == null)
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
                if(order == null)
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
                if(delivery == null)
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
                        LONGITUDE_OBRA = csvRow.LONGITUDE_OBRA
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

            SimpleHeuristicHaversine.Execute(folderPath, loadingPlaces, mixerTrucks,
            orders, deliveries, trafficInfo,
            DEFAULT_DIESEL_COST, DEFAULT_RMC_COST, FIXED_MIXED_TRUCK_COST,
            FIXED_MIXED_TRUCK_CAPACIT_M3, FIXED_L_PER_KM, FIXED_LOADING_TIME,
            FIXED_CUSTOMER_FLOW_RATE);

        }
    }
}
