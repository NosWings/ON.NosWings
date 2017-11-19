using System.Collections.Generic;
using OpenNos.Domain;

namespace OpenNos.GameObject.Helpers
{
    public class PassiveSkillHelper
    {
        public List<BCard> PassiveSkillToBcards(IEnumerable<CharacterSkill> skills)
        {
            List<BCard> bcards = new List<BCard>();

            foreach (CharacterSkill skill in skills)
            {
                switch (skill.Skill.CastId)
                {
                    case 4:
                        bcards.Add(new BCard
                        {
                            FirstData = skill.Skill.UpgradeSkill,
                            Type = (byte)BCardType.CardType.MaxHPMP,
                            SubType = (byte)AdditionalTypes.MaxHPMP.MaximumHPIncreased,
                        });
                        break;
                    case 5:
                        bcards.Add(new BCard
                        {
                            FirstData = skill.Skill.UpgradeSkill,
                            Type = (byte)BCardType.CardType.MaxHPMP,
                            SubType = (byte)AdditionalTypes.MaxHPMP.MaximumMPIncreased,
                        });
                        break;
                    case 8:
                        bcards.Add(new BCard
                        {
                            FirstData = skill.Skill.UpgradeSkill,
                            Type = (byte)BCardType.CardType.Recovery,
                            SubType = (byte)AdditionalTypes.Recovery.HPRecoveryIncreased,
                        });
                        break;
                    case 9:
                        bcards.Add(new BCard
                        {
                            FirstData = skill.Skill.UpgradeSkill,
                            Type = (byte)BCardType.CardType.Recovery,
                            SubType = (byte)AdditionalTypes.Recovery.MPRecoveryIncreased,
                        });
                        break;
                }

            }
            return bcards;
        }

        #region Singleton

        private static PassiveSkillHelper _instance;

        public static PassiveSkillHelper Instance
        {
            get { return _instance ?? (_instance = new PassiveSkillHelper()); }
        }

        #endregion
    }
}
