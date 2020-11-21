using System;
using System.Collections.Generic;

namespace Heuristics.Entities
{
    public class MixerTruck
    {
        public int index;
        public int CODVEICULO;
        public int CODCENTCUS;
        public double LATITUDE_FILIAL;
        public double LONGITUDE_FILIAL;

        public DateTime EndOfTheLastService = DateTime.MinValue;

        public MixerTruck Clone()
        {
            return new MixerTruck()
            {
                index = this.index,
                CODVEICULO = this.CODVEICULO,
                CODCENTCUS = this.CODCENTCUS,
                LATITUDE_FILIAL = this.LATITUDE_FILIAL,
                LONGITUDE_FILIAL = this.LONGITUDE_FILIAL,
                EndOfTheLastService = new DateTime(this.EndOfTheLastService.Ticks)
            };
        }
    }
}
