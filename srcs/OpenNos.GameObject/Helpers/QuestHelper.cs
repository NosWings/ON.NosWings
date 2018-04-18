using System.Collections.Generic;

namespace OpenNos.GameObject.Helpers
{
    public class QuestHelper
    {
        #region Instantiation

        public QuestHelper()
        {
            LoadSkipQuests();
        }

        #endregion

        #region Properties

        public List<int> SkipQuests { get; set; }

        #endregion

        #region Methods

        public void LoadSkipQuests()
        {
            SkipQuests = new List<int>();
            SkipQuests.AddRange(new List<int> { 1676, 1677, 1698, 1714, 1715, 1719, 3014, 3019 });
        }

        #endregion

        #region Singleton

        private static QuestHelper _instance;

        public static QuestHelper Instance => _instance ?? (_instance = new QuestHelper());

        #endregion
    }
}