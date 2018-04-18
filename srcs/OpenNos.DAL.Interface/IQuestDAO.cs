using System.Collections.Generic;
using OpenNos.Data;

namespace OpenNos.DAL.Interface
{
    public interface IQuestDAO : IMappingBaseDAO
    {
        #region Methods

        void InsertOrUpdate(List<QuestDTO> quests);
        QuestDTO Insert(QuestDTO quest);

        void Insert(List<QuestDTO> quests);

        List<QuestDTO> LoadAll();

        QuestDTO LoadById(long questId);

        #endregion
    }
}