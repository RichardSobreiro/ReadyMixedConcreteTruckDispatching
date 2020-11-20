using Heuristics.Entities;
using System.Collections.Generic;

namespace Heuristics
{
    public static class CreateCplexInstance
    {
        public static Instance GetCplexInstance(List<LoadingPlace> loadingPlaces, List<MixerTruck> mixerTrucks,
            List<Delivery> deliveries, float FIXED_MIXED_TRUCK_CAPACIT_M3, float FIXED_MIXED_TRUCK_COST)
        {
            Instance instance = new Instance();
            instance.nLP = loadingPlaces.Count;
            instance.nMT = mixerTrucks.Count;
            instance.nD = deliveries.Count;
            instance.dcod = new int[deliveries.Count];
            instance.odcod = new int[deliveries.Count];
            instance.lpmt = new int[mixerTrucks.Count];
            instance.c = new float[mixerTrucks.Count][];
            instance.t = new float[mixerTrucks.Count][];
            instance.q = FIXED_MIXED_TRUCK_CAPACIT_M3;
            instance.tc = FIXED_MIXED_TRUCK_COST;
            instance.d = new float[deliveries.Count];
            instance.a = new float[deliveries.Count];
            instance.b = new float[deliveries.Count];
            instance.cfr = new float[deliveries.Count];
            instance.od = new float[deliveries.Count];
            instance.dmbs = new float[deliveries.Count];
            instance.dmt = new float[mixerTrucks.Count][];

            instance.r = new float[deliveries.Count];

            instance.ld = 8;

            instance.fdno = 0;

            instance.M = 720;

            for (int i = 0; i < mixerTrucks.Count; i++)
            {
                instance.c[i] = new float[deliveries.Count];
                instance.t[i] = new float[deliveries.Count];
                instance.dmt[i] = new float[deliveries.Count];
                foreach (Delivery delivery in deliveries)
                {

                }
            }

            return instance;
        }
    }
}
