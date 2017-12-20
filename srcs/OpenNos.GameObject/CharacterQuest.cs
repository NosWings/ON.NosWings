using System.Collections.Generic;
using OpenNos.Data;
using OpenNos.GameObject.Networking;
using System.Collections.Generic;
using System.Linq;
using NosSharp.Enums;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject
{
    public class CharacterQuest : CharacterQuestDTO
    {
        #region Members

        private Quest _quest;

        #endregion

        #region Instantiation

        public CharacterQuest()
        {

        }

        public CharacterQuest(long questId, long characterId)
        {
            QuestId = questId;
            CharacterId = characterId;
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

        public string GetInfoPacket(bool sendMsg)
        {
            return $"{QuestNumber}.{Quest.InfoId}.{Quest.InfoId}.{Quest.QuestType}.{FirstObjective}.{GetObjectiveByIndex(1)?.Objective ?? 0}.{(RewardInWaiting ? 1 : 0)}.{SecondObjective}.{GetObjectiveByIndex(2)?.Objective ?? 0}.{ThirdObjective}.{GetObjectiveByIndex(3)?.Objective ?? 0}.{FourthObjective}.{GetObjectiveByIndex(4)?.Objective ?? 0}.{FifthObjective}.{GetObjectiveByIndex(5)?.Objective ?? 0}.{(sendMsg ? 1 : 0)}";
        }

        public QuestObjectiveDTO GetObjectiveByIndex(byte index)
        {
            return Quest.QuestObjectives.FirstOrDefault(q => q.ObjectiveIndex.Equals(index));
        }

        public int[] GetObjectives()
        {
            return new int[] { FirstObjective, SecondObjective, ThirdObjective, FourthObjective, FifthObjective };
        }

        public void Incerment(byte index, int amount)
        {
            switch (index)
            {
                case 1:
                    FirstObjective += FirstObjective >= GetObjectiveByIndex(index)?.Objective ? 0 : amount;
                    break;

                case 2:
                    SecondObjective += SecondObjective >= GetObjectiveByIndex(index)?.Objective ? 0 : amount;
                    break;

                case 3:
                    ThirdObjective += ThirdObjective >= GetObjectiveByIndex(index)?.Objective ? 0 : amount;
                    break;

                case 4:
                    FourthObjective += FourthObjective >= GetObjectiveByIndex(index)?.Objective ? 0 : amount;
                    break;

                case 5:
                    FifthObjective += FifthObjective >= GetObjectiveByIndex(index)?.Objective ? 0 : amount;
                    break;
            }
        }

        public override void Initialize()
        {

        }

        #endregion
    }
}
