using System;

namespace OpenNos.Master.Library.Data
{
    public class PercentBar
    {
        private readonly DateTime _nextMonth;

        private int _percentage;

        private short _totalTime;

        private DateTime _latestUpdate;

        public PercentBar()
        {
            DateTime olddate = DateTime.Now.AddMonths(1);
            _nextMonth = new DateTime(olddate.Year, olddate.Month, 1, 0, 0, 0, olddate.Kind);
            _latestUpdate = DateTime.Now;
        }

        public int MinutesUntilReset
        {
            get { return (int)(_nextMonth - DateTime.Now).TotalMinutes; }
        }

        public byte Mode { get; set; }

        public int Percentage
        {
            get { return Mode == 0 ? _percentage : 0; }
            set
            {
                _percentage = value;
            }
        }

        public short CurrentTime => Mode == 0 ? (short)0 : (short)(_latestUpdate.AddSeconds(_totalTime) - DateTime.Now).TotalSeconds;

        public short TotalTime
        {
            get => Mode == 0 ? (short)0 : _totalTime;
            set
            {
                _latestUpdate = DateTime.Now;
                _totalTime = value;
            }
        }

        public int KilledMonsters { get; set; }

        public bool IsMorcos { get; set; }

        public bool IsHatus { get; set; }

        public bool IsCalvina { get; set; }

        public bool IsBerios { get; set; }
    }
}
