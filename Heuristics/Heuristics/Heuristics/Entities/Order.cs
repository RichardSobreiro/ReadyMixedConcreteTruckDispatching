using GeoCoordinatePortable;
using System;
using System.Collections.Generic;

namespace Heuristics.Entities
{
    public class Order
    {
        public int CODPROGRAMACAO;
        public int CODCENTCUS;
        public float MEDIA_M3_DESCARGA;
        public float VALTOTALPROGRAMACAO;
        public DateTime HORSAIDACENTRAL;
        public double LATITUDE_OBRA;
        public double LONGITUDE_OBRA;
        public double VLRVENDA;
        public List<Delivery> TRIPS;
        public GeoCoordinate Coordinates;
        public double LoadingPlaceCostHaversine;
        public double LoadingPlaceCostGoogleMaps;
    }
}
