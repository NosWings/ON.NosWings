using OpenNos.Data;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IQuestRewardDAO : IMappingBaseDAO
    {
        #region Methods

        QuestRewardDTO Insert(QuestRewardDTO questReward);

        void Insert(List<QuestRewardDTO> questRewards);

        List<QuestRewardDTO> LoadAll();

        IEnumerable<QuestRewardDTO> LoadByQuestId(long questId);

        #endregion
    }
}
