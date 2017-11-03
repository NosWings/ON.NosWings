using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Domain;

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

        public Guid Id { get; set; }

        public int FirstCurrentObjective { get; set; }

        public int SecondCurrentObjective { get; set; }

        public int ThirdCurrentObjective { get; set; }

        public bool RewardInWaiting { get; set; }

        public List<QuestRewardDTO> QuestRewards { get; set; }

        public byte QuestTypeId { get; set; }

        #endregion

        #region Methods

        public string GetRewardPacket(Character character)
        {
            if (!QuestRewards.Any())
            {
                return string.Empty;
            }

            return $"qr {GetRewardPacket(QuestRewards.FirstOrDefault())} {GetRewardPacket(QuestRewards.Skip(1).FirstOrDefault())} {GetRewardPacket(QuestRewards.Skip(2).FirstOrDefault())} {GetRewardPacket(QuestRewards.Skip(3).FirstOrDefault())} {QuestId}";

            string GetRewardPacket(QuestRewardDTO reward)
            {
                if (reward == null)
                {
                    return "0 0 0";
                }
                switch ((QuestRewardType) reward.RewardType)
                {
                    // Item
                    case QuestRewardType.EquipItem:
                    case QuestRewardType.EtcMainItem:
                        character.GiftAdd((short) reward.Data, (byte) (reward.Amount == 0 ? 1 : reward.Amount), isQuest: true);
                        return $"{reward.RewardType} {reward.Data} {(reward.Amount == 0 ? 1 : reward.Amount)}";

                    //Gold
                    case QuestRewardType.Gold:
                        character.GetGold(reward.Amount, true);
                        return $"{reward.RewardType} 0 {(reward.Amount == 0 ? 1 : reward.Amount)}";

                    default:
                        return "0 0 0";
                }
            }
        }

        public string TargetPacket()
        {
            return $"target {TargetX} {TargetY} {TargetMap} {QuestId}";
        }

        public string RemoveTargetPacket()
        {
            return $"targetoff {TargetX} {TargetY} {TargetMap} {QuestId}";
        }

        #endregion

    }
}
