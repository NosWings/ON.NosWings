using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Act6Stats
    {
        public Act6Stats()
        {
            
        }

        public byte EreniaPercentage { get; set; }

        public byte ZenasPercentage { get; set; }

        public bool IsZenas { get; set; }

        public bool IsErenia { get; set; }

        public int TotalDemonsKilled { get; set; }

        public int TotalAngelsKilled { get; set; }

        public bool IsRaidActive { get; set; }
    }
}
