using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class Act6Stats
    {

        private short _totalTime;

        private DateTime _latestUpdate;
        public Act6Stats()
        {
            
        }

        public short CurrentTime
        {
            get { return IsRaidActive == false ? (short)0 : (short)(_latestUpdate.AddSeconds(_totalTime) - DateTime.Now).TotalSeconds; }
        }

        public short TotalTime
        {
            get { return IsRaidActive == false ? (short)0 : _totalTime; }
            set
            {
                _latestUpdate = DateTime.Now;
                _totalTime = value;
            }
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
