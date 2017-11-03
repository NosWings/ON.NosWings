using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;

namespace OpenNos.GameObject
{
    public class Quest : QuestDTO
    {
        #region Instantiation

        public Quest()
        {

        }

        #endregion

        #region Properties

        public int FirstCurrentObjective { get; set; }

        public int SecondCurrentObjective { get; set; }

        public int ThirdCurrentObjective { get; set; }

        public bool RewardInWaiting { get; set; }

        public List<QuestRewardDTO> QuestRewards { get; set; }

        #endregion
    }
}
