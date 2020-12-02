using GeoCoordinatePortable;
using System;
using System.Collections.Generic;

namespace Heuristics.Entities
{
    public class LoadingPlace
    {
        public int index;
        public int CODCENTCUS;
        public int CODCENTCUSSISTER;
        public double LATITUDE_FILIAL;
        public double LONGITUDE_FILIAL;
        public double DISTANCE_HAVERSINE;
        public int TRAVELTIME_HAVERSINE;
        public double DISTANCE_GOOGLEMAPS;
        public int TRAVELTIME_GOOGLEMAPS;
        public GeoCoordinate Coordinates;
        public double CostHaversine { get; set; }
        public double CostGoogleMaps { get; set; }

        public List<MixerTruck> MixerTrucks;

        public LoadingPlaceInfo CreateInfo() 
        {
            return new LoadingPlaceInfo() 
            {
                index = this.index,
                CODCENTCUS = this.CODCENTCUS,
                CODCENTCUSSISTER = this.CODCENTCUSSISTER,
                LATITUDE_FILIAL = this.LATITUDE_FILIAL,
                LONGITUDE_FILIAL = this.LONGITUDE_FILIAL,
                Distance = 0,
                Cost = 0,
                Deliveries = new List<Delivery>()
            };
        }

        public LoadingPlace Clone()
        {
            return new LoadingPlace()
            {
                index = this.index,
                CODCENTCUS = this.CODCENTCUS,
                LATITUDE_FILIAL = this.LATITUDE_FILIAL,
                LONGITUDE_FILIAL = this.LONGITUDE_FILIAL,
                DISTANCE_HAVERSINE = this.DISTANCE_HAVERSINE,
                TRAVELTIME_HAVERSINE = this.TRAVELTIME_HAVERSINE,
                DISTANCE_GOOGLEMAPS = this.DISTANCE_GOOGLEMAPS,
                TRAVELTIME_GOOGLEMAPS = this.TRAVELTIME_GOOGLEMAPS,
                Coordinates = new GeoCoordinate(this.LATITUDE_FILIAL, this.LONGITUDE_FILIAL),
                CostHaversine = this.CostHaversine,
                CostGoogleMaps = this.CostGoogleMaps,
                MixerTrucks = MixerTrucks == null ? new List<MixerTruck>() : new List<MixerTruck>(MixerTrucks)
            };
        }
    }
}
