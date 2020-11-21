using System.Collections.Generic;

namespace Heuristics.Entities
{
    public class Result
    {
        public int objective;
        public int numberOfLoadingPlaces;
        public int numberOfMixerTrucks;
        public int numberOfDeliveries;
        public List<ResultTrip> trips;
        public class ResultTrip
        {
            public int OrderId;
            public int Delivery;
            public int MixerTruck;
            public int LoadingBeginTime;
            public int ServiceTime;
            public int ReturnTime;
            public int LoadingPlant;
            public int Revenue;
            public int BeginTimeWindow;
            public int EndTimeWindow;
            public int TravelTime;
            public int TravelCost;
            public int DurationOfService;
            public int IfDeliveryMustBeServed;
            public int CodDelivery;
            public int CodOrder;
        }
    }
}
