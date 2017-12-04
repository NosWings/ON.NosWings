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
                    session.Character.GiftAdd(1010, 50); // Potion de guérison 1k / 1k
                    break;
                case 30:
                    session.Character.GiftAdd(1011, 50); // Potions géantes 1k5 / 1k5
                    session.Character.GiftAdd(1452, 1); // béné
                    break;
                case 40:
                    session.Character.GiftAdd(1011, 50); // potions géantes 1k5 / 1k5
                    session.Character.GiftAdd(1452, 2); // Béné
                    session.Character.GiftAdd(1363, 2); // parcho bleu
                    break;
                case 50:
                    session.Character.GiftAdd(1011, 50); // potions géantes 1k5 / 1k5
                    session.Character.GiftAdd(1452, 2); // Béné
                    session.Character.GiftAdd(1363, 2); // parcho bleu
                    session.Character.GiftAdd(1218, 2); // parcho up stuff
                    break;
                case 60:
                    session.Character.GiftAdd(1011, 75); // potions géantes 1k5 / 1k5
                    session.Character.GiftAdd(1452, 3); // Béné
                    session.Character.GiftAdd(1363, 2); // parcho bleu
                    session.Character.GiftAdd(1218, 2); // parcho up stuff
                    break;
                case 70:
                    session.Character.GiftAdd(1244, 30); // TF
                    session.Character.GiftAdd(1452, 4); // Béné
                    session.Character.GiftAdd(1363, 3); // parcho bleu
                    session.Character.GiftAdd(2282, 99);  // plume d'ange
                    break;
                case 80:
                    session.Character.GiftAdd(1244, 60); // TF
                    session.Character.GiftAdd(1452, 5); // Béné
                    session.Character.GiftAdd(1363, 3); // parcho rouge
                    session.Character.GiftAdd(282, 1); // Amu pari
                    break;
                case 90:
                    session.Character.GiftAdd(1244, 99); // TF
                    session.Character.GiftAdd(1452, 5); // Béné
                    session.Character.GiftAdd(1363, 5); // parcho rouge
                    break;
                case 150:
                    session.Character.GiftAdd(1244, 99); // TF
                    session.Character.GiftAdd(1452, 5); // Béné
                    session.Character.GiftAdd(1363, 5); // parcho rouge
                    session.Character.GiftAdd(4262, 1); // Amu pari héroique
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