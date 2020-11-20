using CsvHelper;
using GeoCoordinatePortable;
using Heuristics.Entities;
using Heuristics.Entities.MapsGoogle;
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
            float FIXED_MIXED_TRUCK_CAPACIT_M3 = 10;
            float FIXED_MIXED_TRUCK_COST = 50;

            IEnumerable<CsvRow> records;
            using (var reader = new System.IO.StreamReader(folderPath + "Trips.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records = csv.GetRecords<CsvRow>();
            }

            int loadingPlaceIndex = 1;
            List<LoadingPlace> loadingPlaces = new List<LoadingPlace>();
            int mixerTruckIndex = 1;
            List<MixerTruck> mixerTrucks = new List<MixerTruck>();
            List<Order> orders = new List<Order>();
            List<Delivery> deliveries= new List<Delivery>();
            foreach (CsvRow csvRow in records)
            {
                LoadingPlace loadingPlace = loadingPlaces.FirstOrDefault(lp => lp.CODCENTCUS == csvRow.CODCENTCUSVIAGEM);
                if (loadingPlace.Equals(default(LoadingPlace)))
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
                        VALVOLUMEPROG = csvRow.VALTOTALPROGRAMACAO,
                        CODTRACO = csvRow.CODTRACO,
                        CUSVAR = csvRow.CUSVAR,
                        LATITUDE_OBRA = csvRow.LATITUDE_OBRA,
                        LONGITUDE_OBRA = csvRow.LONGITUDE_OBRA
                    };
                }
            }
            List<DirectionsResult> directionsResults;
            using (StreamReader r = new StreamReader(folderPath + "DirectionsResultsStored.json"))
            {
                string json = r.ReadToEnd();
                directionsResults = JsonSerializer.Deserialize<List<DirectionsResult>>(json);
            }
            foreach (Order order in orders)
            {
                order.TRIPS = deliveries.Where(d => d.CODPROGRAMACAO == order.CODPROGRAMACAO).ToList();
                foreach(LoadingPlace loadingPlace in loadingPlaces)
                {
                    DirectionsResult directionsResult = directionsResults.FirstOrDefault(dr =>
                        dr.OriginLatitude == loadingPlace.LATITUDE_FILIAL &&
                        dr.OriginLongitude == loadingPlace.LONGITUDE_FILIAL &&
                        dr.DestinyLatitude == order.LATITUDE_OBRA &&
                        dr.DestinyLongitude == order.LONGITUDE_OBRA &&
                        dr.Hour == order.HORSAIDACENTRAL.Hour);
                    loadingPlace.DISTANCE_HAVERSINE = loadingPlace.Coordinates.GetDistanceTo(order.Coordinates);
                    loadingPlace.TRAVELTIME_HAVERSINE = (int)(2 * loadingPlace.DISTANCE_HAVERSINE);
                    loadingPlace.DISTANCE_GOOGLEMAPS = directionsResult.Distance;
                    loadingPlace.TRAVELTIME_GOOGLEMAPS = directionsResult.TravelTime;
                    order.LOADINGPLACES_INFO.Add(loadingPlace);
                }
            }

        }
    }
}
