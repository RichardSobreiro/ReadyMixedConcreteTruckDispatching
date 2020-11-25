using System.Collections.Generic;

namespace Heuristics.Entities
{
    public class Result
    {
        public int objective { get; set; }
        public int numberOfLoadingPlaces { get; set; }
        public int numberOfMixerTrucks { get; set; }
        public int numberOfDeliveries { get; set; }
        public List<ResultTrip> trips { get; set; }
        public class ResultTrip
        {
            public int OrderId { get; set; }
            public int Delivery { get; set; }
            public int MixerTruck { get; set; }
            public int LoadingBeginTime { get; set; }
            public int ServiceTime { get; set; }
            public int ReturnTime { get; set; }
            public int LoadingPlant { get; set; }
            public int Revenue { get; set; }
            public int BeginTimeWindow { get; set; }
            public int EndTimeWindow { get; set; }
            public int TravelTime { get; set; }
            public int TravelCost { get; set; }
            public int DurationOfService { get; set; }
            public int IfDeliveryMustBeServed { get; set; }
            public int CodDelivery { get; set; }
            public int CodOrder { get; set; }
            public int Lateness { get; set; }
        }
    }
}
