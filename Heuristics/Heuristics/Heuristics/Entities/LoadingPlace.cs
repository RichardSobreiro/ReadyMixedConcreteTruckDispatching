using GeoCoordinatePortable;

namespace Heuristics.Entities
{
    public class LoadingPlace
    {
        public int index;
        public int CODCENTCUS;
        public double LATITUDE_FILIAL;
        public double LONGITUDE_FILIAL;
        public double DISTANCE_HAVERSINE;
        public int TRAVELTIME_HAVERSINE;
        public double DISTANCE_GOOGLEMAPS;
        public int TRAVELTIME_GOOGLEMAPS;
        public GeoCoordinate Coordinates;
    }
}
