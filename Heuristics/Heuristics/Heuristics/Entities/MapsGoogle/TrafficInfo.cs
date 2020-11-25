using System.Collections.Generic;

namespace Heuristics.Entities.MapsGoogle
{
    public class TrafficInfo
    {
        public List<DirectionsResult> DirectionsResults { get; set; }

        public class DirectionsResult
        {
            public double Distance { get; set; }
            public double TravelTime { get; set; }
            public double OriginLatitude { get; set; }
            public double OriginLongitude { get; set; }
            public double DestinyLatitude { get; set; }
            public double DestinyLongitude { get; set; }
            public int Hour { get; set; }
            public string TimeString { get; set; }
            public string Result { get; set; }
            public bool RealResult { get; set; }
        }
    }
}
