namespace Heuristics.Entities
{
    public struct Instance
    {
        public int nLP; // Number of loading plants
        public int nMT; // Number of concrete truck mixers
        public int nD; // Number of customer's deliveries

        public float[][] c; // Cost of the journey between loading plant of the mixer truck k and the construction site of the delivery i
        public float[][] t; // Duration of the journey between loading plant of the vehicle k and customer construction site i
        public int[] lpmt; // Index of the loading plant which is the base for the mixer truck k.

        public float q; // Concrete truck mixers capacity
        public float tc; // Concrete truck mixers fixed maintenance cost

        public int[] dcod; // Id of the real delivery
        public int[] odcod; // Id of the real order 

        public float[] d; // Demand for RMC at customer delivery i
        public float[] a; // Begin for the time window at customer delivery i 
        public float[] b; // End for the time window at customer delivery i
        public float[] cfr; // Concrete flow rate at customer i
        public float[] od; // Order id of delivery i
        public float[] dmbs; // If delivery must be served
        public float[][] dmt; // If the type of RMC of the delivery i is available at the base loading plant of the mixer truck k 

        public float[] r; // Profit obtained from attending customer delivery i

        public float ld; // Loading time duration for each delivery

        public int fdno; // Index of the first delivery of the new order

        public float M; // Big M for delivery i and j
    }
}
