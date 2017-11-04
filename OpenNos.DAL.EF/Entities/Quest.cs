using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.DAL.EF
{
    public class Quest
    {
        #region Properties

        public long QuestId { get; set; }

        public byte QuestType { get; set; }

        public int FirstData { get; set; }

        public int? SecondData { get; set; }

        public int? ThirdData { get; set; }

        public int FirstObjective { get; set; }

        public int? SecondObjective { get; set; }

        public int? ThirdObjective { get; set; }

        public short? TargetMap { get; set; }

        public short? TargetX { get; set; }

        public short? TargetY { get; set; }

        public long? NextQuestId { get; set; }

        #endregion
    }
}
