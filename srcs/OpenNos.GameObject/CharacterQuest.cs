using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Domain;

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
            LoadData();
        }

        public CharacterQuest(CharacterQuestDTO characterQuestDto)
        {
            Id = characterQuestDto.Id;
            FirstObjective = characterQuestDto.FirstObjective;
            SecondObjective = characterQuestDto.SecondObjective;
            ThirdObjective = characterQuestDto.ThirdObjective;
            FourthObjective = characterQuestDto.FourthObjective;
            QuestId = characterQuestDto.QuestId;
            CharacterId = characterQuestDto.CharacterId;
            IsMainQuest = characterQuestDto.IsMainQuest;
            LoadData();
        }

        #endregion

        #region Properties

        public Dictionary<byte, int[]> Data { get; set; }

        public Quest Quest
        {
            get { return _quest ?? (_quest = ServerManager.Instance.GetQuest(QuestId)); }
        }

        public bool RewardInWaiting { get; set; }

        public List<QuestRewardDTO> QuestRewards { get; set; }

        public short QuestNumber { get; set; }

        #endregion

        #region Methods

        public void LoadData()
        {
            Data = new Dictionary<byte, int[]>();
            Data.Add(1, new int[] { Quest.FirstData, Quest.FirstSpecialData ?? -1, Quest.FirstObjective});
            Data.Add(2, new int[] { Quest.SecondData ?? -1, Quest.SecondSpecialData ?? -1, Quest.SecondObjective ?? 0 });
            Data.Add(3, new int[] { Quest.ThirdData ?? -1, Quest.ThirdSpecialData ?? -1, Quest.ThirdObjective ?? 0 });
            Data.Add(4, new int[] { Quest.FourthData ?? -1, Quest.FourthSpecialData ?? -1, Quest.FourthObjective ?? 0 });
        }
        #endregion
    }
}
