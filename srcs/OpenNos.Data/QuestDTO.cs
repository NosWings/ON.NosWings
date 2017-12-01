using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Data
{
    public class QuestDTO : MappingBaseDTO
    {
        #region Properties

        public long QuestId { get; set; }

        public int QuestType { get; set; }

        public byte LevelMin { get; set; }

        public byte LevelMax { get; set; }

        public int? StartDialogId { get; set; }

        public int? EndDialogId { get; set; }

        public int FirstData { get; set; }

        public int FirstObjective { get; set; }

        public int? FirstSpecialData { get; set; }

        public int? SecondData { get; set; }

        public int? SecondObjective { get; set; }

        public int? SecondSpecialData { get; set; }

        public int? ThirdData { get; set; }

        public int? ThirdObjective { get; set; }

        public int? ThirdSpecialData { get; set; }

        public int? FourthData { get; set; }

        public int? FourthObjective { get; set; }

        public int? FourthSpecialData { get; set; }

        public short? TargetMap { get; set; }

        public short? TargetX { get; set; }

        public short? TargetY { get; set; }

        public int InfoId { get; set; }

        public long? NextQuestId { get; set; }

        public bool IsDaily { get; set; }

        public int? SpecialData { get; set; }

        #endregion
    }
}
