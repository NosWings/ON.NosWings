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

        public List<QuestRewardDTO> QuestRewards { get; set; }

        #endregion
    }
}
