using System.Collections.Generic;
using System.Linq;
using NosSharp.Enums;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

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

            return $"qr {GetRewardPacket()} {InfoId}";

            string GetRewardPacket()
            {
                string str = "";
                for (int a = 0; a < 4; a++)
                {
                    QuestRewardDTO reward = QuestRewards.Skip(a).FirstOrDefault();
                    if (reward == null)
                    {
                        str += "0 0 0 ";
                        continue;
                    }
                    switch ((QuestRewardType)reward.RewardType)
                    {
                        // Item
                        case QuestRewardType.WearItem:
                        case QuestRewardType.EtcMainItem:
                            character.GiftAdd((short)reward.Data, (byte)(reward.Amount == 0 ? 1 : reward.Amount), reward.Design, reward.Upgrade, (sbyte)reward.Rarity, true);
                            str += $"{reward.RewardType} {reward.Data} {(reward.Amount == 0 ? 1 : reward.Amount)} ";
                            break;

                        // Gold
                        case QuestRewardType.Gold:
                        case QuestRewardType.SecondGold:
                        case QuestRewardType.ThirdGold:
                        case QuestRewardType.FourthGold:
                            character.GetGold(reward.Amount, true);
                            str += $"{reward.RewardType} 0 {(reward.Amount == 0 ? 1 : reward.Amount)} ";
                            break;

                        case QuestRewardType.Reput: // Reputation
                            character.GetReput(reward.Amount);
                            str += $"{reward.RewardType} 0 0";
                            break;

                        case QuestRewardType.Exp: // Experience
                            if (character.Level >= ServerManager.Instance.MaxLevel)
                            {
                                str += "0 0 0 ";
                                break;
                            }
                            character.GetXp((long)(CharacterHelper.Instance.XpData[reward.Data > 255 ? 255 : reward.Data] * reward.Amount / 100D));
                            str += $"{reward.RewardType} 0 0 ";
                            break;

                        case QuestRewardType.SecondExp: // % Experience
                            if (character.Level >= ServerManager.Instance.MaxLevel)
                            {
                                str += "0 0 0 ";
                                break;
                            }
                            character.GetXp((long)(CharacterHelper.Instance.XpData[character.Level] * reward.Amount / 100D));
                            str += $"{reward.RewardType} 0 0 ";
                            break;

                        case QuestRewardType.JobExp: // JobExperience
                            character.GetJobExp((long)((character.Class == (byte)ClassType.Adventurer ? CharacterHelper.Instance.FirstJobXpData[reward.Data > 255 ? 255 : reward.Data] : CharacterHelper.Instance.SecondJobXpData[reward.Data > 255 ? 255 : reward.Data]) * reward.Amount / 100D));
                            str += $"{reward.RewardType} 0 0 ";
                            break;

                        case QuestRewardType.SecondJobExp: // % JobExperience
                            character.GetJobExp((long)((character.Class == (byte)ClassType.Adventurer ? CharacterHelper.Instance.FirstJobXpData[character.JobLevel] : CharacterHelper.Instance.SecondJobXpData[character.JobLevel]) * reward.Amount / 100D));
                            str += $"{reward.RewardType} 0 0 ";
                            break;

                        default:
                            str += "0 0 0 ";
                            break;
                    }
                }
                return str;
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
