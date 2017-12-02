using System;

namespace OpenNos.Master.Controllers
{
    internal class GlacernonController
    {
        #region Members

        private static GlacernonController _instance;

        private readonly DateTime _nextMonth;

        private int _percentage;

        private short _totalTime;

        private DateTime _latestUpdate;

        #endregion

        public static GlacernonController Instance
        {
            get { return _instance ?? (_instance = new GlacernonController()); }
        }

        #region Properties

        public GlacernonController()
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

        public short CurrentTime
        {
            get { return Mode == 0 ? (short)0 : (short)(_latestUpdate.AddSeconds(_totalTime) - DateTime.Now).TotalSeconds; }
        }

        public short TotalTime
        {
            get { return Mode == 0 ? (short)0 : _totalTime; }
            set
            {
                _latestUpdate = DateTime.Now;
                _totalTime = value;
            }
        }

        public int AngelPercentage { get; set; }

        public int DemonPercentage { get; set; }

        public bool IsRaidActive { get; set; }

        #endregion
    }
}
