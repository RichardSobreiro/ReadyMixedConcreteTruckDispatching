using System.Collections.Generic;

namespace Heuristics.Entities
{
    public class LoadingPlaceInfo
    {
        public int index;
        public int CODCENTCUS;
        public int CODCENTCUSSISTER;
        public double LATITUDE_FILIAL;
        public double LONGITUDE_FILIAL;
        public double Distance;
        public int TravelTime;

        public double Cost;

        public List<Delivery> Deliveries;
    }
}
