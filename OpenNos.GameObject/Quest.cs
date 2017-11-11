using System.Collections.Generic;
using System.Linq;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;

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

        #region Methods

        public string GetRewardPacket(Character character)
        {
            if (!QuestRewards.Any())
            {
                return string.Empty;
            }

            return $"qr {GetRewardPacket(QuestRewards.FirstOrDefault())} {GetRewardPacket(QuestRewards.Skip(1).FirstOrDefault())} {GetRewardPacket(QuestRewards.Skip(2).FirstOrDefault())} {GetRewardPacket(QuestRewards.Skip(3).FirstOrDefault())} {InfoId}";

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
                        character.GiftAdd((short) reward.Data, (byte) (reward.Amount == 0 ? 1 : reward.Amount), reward.Design, reward.Upgrade, (sbyte) reward.Rarity, true);
                        return $"{reward.RewardType} {reward.Data} {(reward.Amount == 0 ? 1 : reward.Amount)}";

                    // Gold
                    case QuestRewardType.Gold:
                        character.GetGold(reward.Amount, true);
                        return $"{reward.RewardType} 0 {(reward.Amount == 0 ? 1 : reward.Amount)}";

                    // Reputation
                    case QuestRewardType.Reput:
                        character.GetReput(reward.Amount);
                        return $"{reward.RewardType} 0 0";

                    // Experience
                    case QuestRewardType.Exp:
                        if (reward.Data > 255)
                        {
                            return "0 0 0";
                        }
                        character.GetXp((long) (CharacterHelper.Instance.XpData[reward.Data] / 100D * reward.Amount));
                        return $"{reward.RewardType} 0 0";

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
