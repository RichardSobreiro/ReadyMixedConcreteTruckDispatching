using System;

namespace Heuristics.Entities
{
    public class CsvRow
    {
        public int CODCENTCUS { get; set; }
        public double LATITUDE_FILIAL { get; set; }
        public double LONGITUDE_FILIAL { get; set; }
        public int CODVEICULO { get; set; }
        public int CODPROGRAMACAO { get; set; } 
        public float MEDIA_M3_DESCARGA { get; set; } 
        public float VALTOTALPROGRAMACAO { get; set; } 
        public DateTime HORSAIDACENTRAL { get; set; } 
        public double LATITUDE_OBRA { get; set; } 
        public double LONGITUDE_OBRA { get; set; }
        public double VLRVENDA { get; set; }
        public DateTime HORCHEGADAOBRA { get; set; } 
        public int CODPROGVIAGEM { get; set; } 
        public int CODCENTCUSVIAGEM { get; set; }
        public float VLRTOTALNF { get; set; }
        public float VALVOLUMEPROG { get; set; }
        public double CUSVAR { get; set; }
        public int CODTRACO { get; set; }
    }
}
