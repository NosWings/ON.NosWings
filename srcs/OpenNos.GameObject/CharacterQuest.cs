using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class CharacterQuest : CharacterQuestDTO
    {
        #region Members

        private Quest _quest;

        #endregion

        #region Instantiation

        public CharacterQuest(long questId, long characterId)
        {
            QuestId = questId;
            CharacterId = characterId;
        }

        public CharacterQuest(CharacterQuestDTO characterQuestDto)
        {
            Id = characterQuestDto.Id;
            FirstObjective = characterQuestDto.FirstObjective;
            SecondObjective = characterQuestDto.SecondObjective;
            ThirdObjective = characterQuestDto.ThirdObjective;
            QuestId = characterQuestDto.QuestId;
            CharacterId = characterQuestDto.CharacterId;
        }

        #endregion

        #region Properties

        public Quest Quest
        {
            get { return _quest ?? (_quest = ServerManager.Instance.GetQuest(QuestId)); }
        }

        public bool RewardInWaiting { get; set; }

        public List<QuestRewardDTO> QuestRewards { get; set; }

        public short QuestNumber { get; set; }

        #endregion

        #region Methods

        #endregion
    }
}
