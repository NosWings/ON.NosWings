using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.DAL;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Helpers
{
    public class MateHelper
    {
        #region Instantiation

        public MateHelper()
        {
            LoadConcentrate();
            LoadHpData();
            LoadMinDamageData();
            LoadMaxDamageData();
            LoadPrimaryMpData();
            LoadSecondaryMpData();
            LoadXpData();
            LoadMateBuffs();
            LoadPetSkills();
        }

        #endregion

        #region Members

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

        // Vnum - CardId
        public Dictionary<int, int> MateBuffs { get; set; }

        public List<int> PetSkills { get; set; }

        #endregion

        #region Methods

        #region PetBuffs

        public void AddPetBuff(ClientSession session, Mate mate)
        {
            int cardId = -1;
            if (MateBuffs.TryGetValue(mate.NpcMonsterVNum, out cardId) && session.Character.Buff.All(b => b.Card.CardId != cardId))
            {
                session.Character.AddBuff(new Buff.Buff(cardId, isPermaBuff: true));
            }
            foreach (NpcMonsterSkill skill in mate.Monster.Skills.Where(sk => PetSkills.Contains(sk.SkillVNum)))
            {
                session.SendPacket(session.Character.GeneratePetskill(skill.SkillVNum));
            }
        }

        public void RemovePetBuffs(ClientSession session)
        {
            foreach (Buff.Buff mateBuff in session.Character.BattleEntity.Buffs.Where(b => MateBuffs.Values.Any(v => v == b.Card.CardId)))
            {
                session.Character.RemoveBuff(mateBuff.Card.CardId, true);
            }
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
            int baseHp = 150;
            int hpBaseUp = 40;
            for (int i = 0; i < HpData.Length; i++)
            {
                HpData[i] = baseHp;
                hpBaseUp += 5;
                baseHp += hpBaseUp;
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

        #region List

        private void LoadPetSkills()
        {
            PetSkills = new List<int>
            {
                1513, // Purcival 
                1514, // Baron scratch ?
                1515, // Amiral (le chat chelou) 
                1516, // roi des pirates pussifer 
                1524 // Miaou fou
            };
        }

        private void LoadMateBuffs()
        {
            MateBuffs = new Dictionary<int, int>
            {
                {178, 108}, // LUCKY PIG 
                {670, 374}, // FIBI 
                {830, 377}, // RUDY LOUBARD 
                {836, 381}, // PADBRA
                {838, 385}, // RATUFU NAVY 
                {840, 442}, // LEO LE LACHE 
                {841, 394}, // RATUFU NINJA 
                {842, 399}, // RATUFU INDIEN 
                {843, 403}, // RATUFU VIKING 
                {844, 391}, // RATUFU COWBOY 
                {2105, 383} // INFERNO 
            };
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