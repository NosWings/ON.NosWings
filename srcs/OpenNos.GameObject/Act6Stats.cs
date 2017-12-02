using System;

namespace OpenNos.GameObject
{
    public class Act6Stats
    {

        private short _totalTime;

        private DateTime _latestUpdate;

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

        public byte Percentage { get; set; }

        public int KilledMonsters { get; set; }

        public bool IsRaidActive { get; set; }
    }
}
