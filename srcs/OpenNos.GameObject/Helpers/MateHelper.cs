using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject.Helpers
{
    public class MateHelper
    {
        #region Instantiation

        #endregion

        #region Members

        public MateHelper()
        {
            LoadConcentrate();
            LoadHpData();
            LoadMinDamageData();
            LoadMaxDamageData();
            LoadPrimaryMpData();
            LoadSecondaryMpData();
            LoadXpData();
        }

        #endregion

        #region Properties

        public short[,] Concentrate { get; private set; }

        public int[] HpData { get; private set; }

        public short[,] MinDamageData { get; private set; }

        public short[,] MaxDamageData { get; private set; }

        // Race == 0
        public int[] PrimaryMpData { get; private set; }

        // Race == 2
        public int[] SecondaryMpData { get; private set; }

        public double[] XpData { get; private set; }

        #endregion

        #region Methods

        #region PetBuffs

        // TODO NEED TO FIND A WAY TO APPLY BUFFS PROPERLY THROUGH MONSTER SKILLS
        public void AddPetBuff(ClientSession session)
        {
            IEnumerable<Mate> equipMates = session.Character.Mates.Where(s => s.IsTeamMember);
            IEnumerable<Mate> mates = equipMates as IList<Mate> ?? equipMates.ToList();
            // FIBI
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 670) && session.Character.Buff.All(s => s.Card.CardId != 374))
            {
                session.Character.AddBuff(new Buff(374), false);
            }
            // PADBRA
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 836) && session.Character.Buff.All(s => s.Card.CardId != 381))
            {
                session.Character.AddBuff(new Buff(381), false);
            }
            // INFERNO
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 2105) && session.Character.Buff.All(s => s.Card.CardId != 383))
            {
                session.Character.AddBuff(new Buff(383), false);
            }
            // LUCKY PIG
            if ((mates.Any(s => s.Monster.NpcMonsterVNum == 178) || mates.Any(s => s.Monster.NpcMonsterVNum == 536)) && session.Character.Buff.All(s => s.Card.CardId != 107))
            {
                session.Character.AddBuff(new Buff(108), false);
            }
            // RUDY LOUBARD
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 830) && session.Character.Buff.All(s => s.Card.CardId != 377))
            {
                session.Character.AddBuff(new Buff(377), false);
            }
            // RATUFU COWBOY
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 844) && session.Character.Buff.All(s => s.Card.CardId != 391))
            {
                session.Character.AddBuff(new Buff(391), false);
            }
            // RATUFU NAVY
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 838) && session.Character.Buff.All(s => s.Card.CardId != 385))
            {
                session.Character.AddBuff(new Buff(385), false);
            }
            // RATUFU INDIEN
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 842) && session.Character.Buff.All(s => s.Card.CardId != 399))
            {
                session.Character.AddBuff(new Buff(399), false);
            }
            // RATUFU NINJA
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 841) && session.Character.Buff.All(s => s.Card.CardId != 394))
            {
                session.Character.AddBuff(new Buff(394), false);
            }
            // LEO LE LACHE
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 840) && session.Character.Buff.All(s => s.Card.CardId != 442))
            {
                session.Character.AddBuff(new Buff(442), false);
            }
            // RATUFU VIKING
            if (mates.Any(s => s.Monster.NpcMonsterVNum == 843) && session.Character.Buff.All(s => s.Card.CardId != 403))
            {
                session.Character.AddBuff(new Buff(403), false);
            }
            // Miaou fou
            if (mates.Any(s => s.Monster.Skills.Any(sk => sk.SkillVNum == 1524)))
            {
                session.SendPacket(session.Character.GeneratePetskill(1524));
            }
            // roi des pirates pussifer
            if (mates.Any(s => s.Monster.Skills.Any(sk => sk.SkillVNum == 1516)))
            {
                session.SendPacket(session.Character.GeneratePetskill(1516));
            }
            // Amiral (le chat chelou)
            if (mates.Any(s => s.Monster.Skills.Any(sk => sk.SkillVNum == 1515)))
            {
                session.SendPacket(session.Character.GeneratePetskill(1515));
            }
            // Baron scratch ? 
            if (mates.Any(s => s.Monster.Skills.Any(sk => sk.SkillVNum == 1514)))
            {
                session.SendPacket(session.Character.GeneratePetskill(1514));
            }
            // Purcival
            if (mates.Any(s => s.Monster.Skills.Any(sk => sk.SkillVNum == 1513)))
            {
                session.SendPacket(session.Character.GeneratePetskill(1513));
            }
        }

        public void RemovePetBuffs(ClientSession session)
        {
            session.Character.RemoveBuff(374);
            session.Character.RemoveBuff(381);
            session.Character.RemoveBuff(383);
            session.Character.RemoveBuff(108);
            session.Character.RemoveBuff(377);
            session.Character.RemoveBuff(391);
            session.Character.RemoveBuff(385);
            session.Character.RemoveBuff(399);
            session.Character.RemoveBuff(394);
            session.Character.RemoveBuff(442);
            session.Character.RemoveBuff(403);
            session.SendPacket(session.Character.GeneratePetskill());
        }

        #endregion PetBuffs

        #region Concentrate

        private void LoadConcentrate()
        {
            Concentrate = new short[2, 256];

            short baseConcentrate = 27;
            short baseUp = 6;

            Concentrate[0, 0] = baseConcentrate;

            for (int i = 1; i < Concentrate.GetLength(1); i++)
            {
                Concentrate[0, i] = baseConcentrate;
                baseConcentrate += (short)(i % 5 == 2 ? 5 : baseUp);
            }

            baseConcentrate = 70;

            Concentrate[1, 0] = baseConcentrate;

            for (int i = 1; i < Concentrate.GetLength(1); i++)
            {
                Concentrate[1, i] = baseConcentrate;
            }
        }

        #endregion

        #region HP

        private void LoadHpData()
        {
            HpData = new int[256];
            int baseHp = 138;
            int hpBaseUp = 18;
            for (int i = 0; i < HpData.Length; i++)
            {
                HpData[i] = baseHp;
                hpBaseUp++;
                baseHp += hpBaseUp;

                if (i == 37)
                {
                    baseHp = 1765;
                    hpBaseUp = 65;
                }
                if (i < 41)
                {
                    continue;
                }
                if (((99 - i) % 8) == 0)
                {
                    hpBaseUp++;
                }
            }
        }

        #endregion

        #region Damage

        private void LoadMinDamageData()
        {
            MinDamageData = new short[2, 256];

            short baseDamage = 37;
            short baseUp = 4;

            MinDamageData[0, 0] = baseDamage;

            for (int i = 1; i < MinDamageData.GetLength(1); i++)
            {
                MinDamageData[0, i] = baseDamage;
                baseDamage += (short)(i % 5 == 0 ? 5 : baseUp);
            }

            baseDamage = 23;
            baseUp = 6;

            MinDamageData[1, 0] = baseDamage;

            for (int i = 1; i < MinDamageData.GetLength(1); i++)
            {
                MinDamageData[1, i] = baseDamage;
                baseDamage += (short)(i % 5 == 0 ? 5 : baseUp);
                baseDamage += (short)(i % 2 == 0 ? 1 : 0);
            }
        }

        private void LoadMaxDamageData()
        {
            MaxDamageData = new short[2, 256];

            short baseDamage = 40;
            short baseUp = 6;

            MaxDamageData[0, 0] = baseDamage;

            for (int i = 1; i < MaxDamageData.GetLength(1); i++)
            {
                MaxDamageData[0, i] = baseDamage;
                baseDamage += (short)(i % 5 == 0 ? 5 : baseUp);
            }

            MaxDamageData[1, 0] = baseDamage;

            baseDamage = 38;
            baseUp = 8;

            for (int i = 1; i < MaxDamageData.GetLength(1); i++)
            {
                MaxDamageData[1, i] = baseDamage;
                baseDamage += (short)(i % 5 == 0 ? 5 : baseUp);
            }
        }

        #endregion

        #region MP

        private void LoadPrimaryMpData()
        {
            PrimaryMpData = new int[256];
            PrimaryMpData[0] = 10;
            PrimaryMpData[1] = 10;
            PrimaryMpData[2] = 15;

            int baseUp = 5;
            byte count = 0;
            bool isStable = true;
            bool isDouble = false;

            for (int i = 3; i < PrimaryMpData.Length; i++)
            {
                if (i % 10 == 1)
                {
                    PrimaryMpData[i] += PrimaryMpData[i - 1] + baseUp * 2;
                    continue;
                }
                if (!isStable)
                {
                    baseUp++;
                    count++;

                    if (count == 2)
                    {
                        if (isDouble)
                        {
                            isDouble = false;
                        }
                        else
                        {
                            isStable = true;
                            isDouble = true;
                            count = 0;
                        }
                    }

                    if (count == 4)
                    {
                        isStable = true;
                        count = 0;
                    }
                }
                else
                {
                    count++;
                    if (count == 2)
                    {
                        isStable = false;
                        count = 0;
                    }
                }
                PrimaryMpData[i] = PrimaryMpData[i - (i % 10 == 2 ? 2 : 1)] + baseUp;
            }
        }

        private void LoadSecondaryMpData()
        {
            SecondaryMpData = new int[256];
            SecondaryMpData[0] = 60;
            SecondaryMpData[1] = 60;
            SecondaryMpData[2] = 78;

            int baseUp = 18;
            bool boostUp = false;

            for (int i = 3; i < SecondaryMpData.Length; i++)
            {
                if (i % 10 == 1)
                {
                    SecondaryMpData[i] += SecondaryMpData[i - 1] + i + 10;
                    continue;
                }

                if (boostUp)
                {
                    baseUp += 3;
                    boostUp = false;
                }
                else
                {
                    baseUp++;
                    boostUp = true;
                }

                SecondaryMpData[i] = SecondaryMpData[i - (i % 10 == 2 ? 2 : 1)] + baseUp;
            }
        }

        #endregion

        #region XP

        private void LoadXpData()
        {
            // Load XpData
            XpData = new double[256];
            double[] v = new double[256];
            double var = 1;
            v[0] = 540;
            v[1] = 960;
            XpData[0] = 300;
            for (int i = 2; i < v.Length; i++)
            {
                v[i] = v[i - 1] + 420 + 120 * (i - 1);
            }
            for (int i = 1; i < XpData.Length; i++)
            {
                if (i < 79)
                {
                    switch (i)
                    {
                        case 14:
                            var = 6 / 3d;
                            break;
                        case 39:
                            var = 19 / 3d;
                            break;
                        case 59:
                            var = 70 / 3d;
                            break;
                    }
                    XpData[i] = Convert.ToInt64(XpData[i - 1] + var * v[i - 1]);
                }
                if (i < 79)
                {
                    continue;
                }
                switch (i)
                {
                    case 79:
                        var = 5000;
                        break;
                    case 82:
                        var = 9000;
                        break;
                    case 84:
                        var = 13000;
                        break;
                }
                XpData[i] = Convert.ToInt64(XpData[i - 1] + var * (i + 2) * (i + 2));
            }
        }

        #endregion

        #endregion

        #region Singleton

        private static MateHelper _instance;

        public static MateHelper Instance
        {
            get { return _instance ?? (_instance = new MateHelper()); }
        }

        #endregion
    }
}