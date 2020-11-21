using System;
using System.Collections.Generic;

namespace Heuristics.Entities
{
    public class Delivery
    {
        public DateTime HORCHEGADAOBRA;
        public int CODPROGRAMACAO;
        public int CODPROGVIAGEM;
        public int CODCENTCUSVIAGEM;
        public float VLRTOTALNF;
        public float VALVOLUMEPROG;
        public int CODTRACO;
        public double CUSVAR;
        public double LATITUDE_OBRA;
        public double LONGITUDE_OBRA;
        public double VLRVENDA;
        public List<LoadingPlace> LOADINGPLACES_INFO = new List<LoadingPlace>();

        public DateTime BeginLoadingTime;
        public DateTime EndLoadingTime;
        public DateTime ArrivalTimeAtConstruction;
        public DateTime DepartureTimeAtConstruction;
        public DateTime ArrivalTimeAtLoadingPlace;
        public int CODVEICULO;

    }
}
