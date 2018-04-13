using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Helpers
{
    public class RewardsHelper
    {
        #region Methods

        public int ArenaXpReward(byte characterLevel)
        {
            if (characterLevel <= 39)
            {
                // 25%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 4);
            }

            if (characterLevel <= 55)
            {
                // 20%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 5);
            }

            if (characterLevel <= 75)
            {
                // 10%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 10);
            }

            if (characterLevel <= 79)
            {
                // 5%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 20);
            }

            if (characterLevel <= 85)
            {
                // 2%
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 50);
            }

            if (characterLevel <= 90)
            {
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 80);
            }

            if (characterLevel <= 93)
            {
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 100);
            }

            if (characterLevel <= 99)
            {
                return (int) (CharacterHelper.Instance.XpData[characterLevel] / 1000);
            }

            return 0;
        }

        public void GetLevelUpRewards(ClientSession session)
        {
            switch (session.Character.Level)
            {
                case 20:
                    session.Character.GiftAdd(1010, 50); // 1K/1K healing pots
                    break;
                case 30:
                    session.Character.GiftAdd(1011, 50); // 1K5/1K5 healing pots
                    session.Character.GiftAdd(1452, 1); // ancelloan's blessing
                    break;
                case 40:
                    session.Character.GiftAdd(1011, 50); // 1K5/1K5 healing pots
                    session.Character.GiftAdd(1452, 2); // ancelloan's blessing
                    session.Character.GiftAdd(1363, 2); // blue sp scroll
                    break;
                case 50:
                    session.Character.GiftAdd(1011, 50); // 1K5/1K5 healing pots
                    session.Character.GiftAdd(1452, 2); // Ancelloan's blessing
                    session.Character.GiftAdd(1363, 2); // Blue sp scroll
                    session.Character.GiftAdd(1218, 2); // equipment scroll
                    break;
                case 60:
                    session.Character.GiftAdd(1011, 75); // 1K5/1K5 healing pots
                    session.Character.GiftAdd(1452, 3); // Ancelloan's blessing
                    session.Character.GiftAdd(1363, 2); // Blue sp scroll
                    session.Character.GiftAdd(1218, 2); // Equipment scroll
                    break;
                case 70:
                    session.Character.GiftAdd(1244, 30); // Full pot
                    session.Character.GiftAdd(1452, 4); // Ancelloan's blessing
                    session.Character.GiftAdd(1363, 3); // Blue sp scroll
                    session.Character.GiftAdd(2282, 99); //  WOA
                    break;
                case 80:
                    session.Character.GiftAdd(1244, 60); // Full pots
                    session.Character.GiftAdd(1452, 5); // Ancelloan's blessing
                    session.Character.GiftAdd(1364, 3); // red sp scroll
                    session.Character.GiftAdd(282, 1); // betting amulet
                    break;
                case 90:
                    session.Character.GiftAdd(1244, 99); // Full pot
                    session.Character.GiftAdd(1452, 5); // Ancelloan's blessing
                    session.Character.GiftAdd(1364, 5); // red sp scroll
                    session.Character.GiftAdd(282, 1); // betting amulet
                    session.Character.GiftAdd(282, 1); // betting amulet
                    break;
            }
        }

        public void GetJobRewards(ClientSession session)
        {
            switch (session.Character.JobLevel)
            {
                case 20:
                    session.SendPacket(
                        UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("RECEIVE_SP"), 0));
                    switch (session.Character.Class)
                    {
                        case ClassType.Swordman:
                            session.Character.GiftAdd(901, 1);
                            break;
                        case ClassType.Archer:
                            session.Character.GiftAdd(903, 1);
                            break;
                        case ClassType.Magician:
                            session.Character.GiftAdd(905, 1);
                            break;
                    }

                    break;
            }
        }

        #endregion

        #region Singleton

        private static RewardsHelper _instance;

        public static RewardsHelper Instance
        {
            get { return _instance ?? (_instance = new RewardsHelper()); }
        }

        #endregion
    }
}