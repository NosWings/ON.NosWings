using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Concurrent;
using static NosSharp.Enums.BCardType;
using NosSharp.Enums;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Helpers;
using OpenNos.Core.Extensions;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Networking;
using OpenNos.Data;
using OpenNos.Core;
using System.Collections.Generic;
using OpenNos.GameObject.Npc;
using OpenNos.GameObject.Event;

namespace OpenNos.GameObject.Battle
{
    public class BattleEntity
    {
        #region instantiation

        public BattleEntity(IBattleEntity entity)
        {
            Entity = entity;
            Session = entity.GetSession();
            Buffs = new ConcurrentBag<Buff.Buff>();
            StaticBcards = new ConcurrentBag<BCard>();
            SkillBcards = new ConcurrentBag<BCard>();
            OnDeathEvents = new ConcurrentBag<EventContainer>();
            OnHitEvents = new ConcurrentBag<EventContainer>();
            ObservableBag = new Dictionary<short, IDisposable>();

            if (Session is Character character)
            {
                Level = character.Level;
                return;
            }
            NpcMonster npcMonster = Session is MapMonster mon ? mon.Monster
                                  : Session is MapNpc npc ? npc.Npc
                                  : Session is Mate mate ? mate.Monster
                                  : null;

            if (npcMonster == null)
            {
                return;
            }
            Level = npcMonster.Level;
            Element = npcMonster.Element;
            ElementRate = npcMonster.ElementRate;
            FireResistance = npcMonster.FireResistance;
            WaterResistance = npcMonster.WaterResistance;
            LightResistance = npcMonster.LightResistance;
            DarkResistance = npcMonster.DarkResistance;
            DefenceRate = npcMonster.DefenceDodge;
            DistanceDefenceRate = npcMonster.DistanceDefenceDodge;
            CloseDefence = npcMonster.CloseDefence;
            RangedDefence = npcMonster.DistanceDefence;
            MagicDefence = npcMonster.MagicDefence;
            AttackUpgrade = npcMonster.AttackUpgrade;
            CriticalRate = npcMonster.CriticalChance;
            Critical = npcMonster.CriticalRate - 30;
            MinDamage = npcMonster.DamageMinimum;
            MaxDamage = npcMonster.DamageMaximum;
            HitRate = npcMonster.Concentrate;
        }

        #endregion

        #region Porperties

        public ConcurrentBag<Buff.Buff> Buffs { get; set; }

        public ConcurrentBag<BCard> StaticBcards { get; set; }

        public ConcurrentBag<BCard> SkillBcards { get; set; }

        public ConcurrentBag<EventContainer> OnDeathEvents { get; set; }

        public ConcurrentBag<EventContainer> OnHitEvents { get; set; }

        public Dictionary<short, IDisposable> ObservableBag { get; set; }

        public object Session { get; set; }

        public IBattleEntity Entity { get; set; }

        public byte Level { get; set; }

        #region Element

        public byte Element { get; set; }

        public int ElementRate { get; set; }

        public int ElementRateSp { get; set; }

        public int FireResistance { get; set; }

        public int WaterResistance { get; set; }

        public int LightResistance { get; set; }

        public int DarkResistance { get; set; }

        #endregion

        #region Attack

        public byte AttackUpgrade { get; set; }

        public int Critical { get; set; }

        public int CriticalRate { get; set; }

        public int MinDamage { get; set; }

        public int MaxDamage { get; set; }

        public int HitRate { get; set; }

        #endregion

        #region Defence

        public byte DefenceUpgrade { get; set; }

        public int DefenceRate { get; set; }

        public int DistanceDefenceRate { get; set; }

        public int CloseDefence { get; set; }

        public int RangedDefence { get; set; }

        public int MagicDefence { get; set; }

        #endregion

        #endregion

        #region Methods

        public void AddBuff(Buff.Buff indicator)
        {
            if (indicator?.Card == null || indicator.Card.BuffType == BuffType.Bad && Buffs.Any(b => b.Card.CardId == indicator.Card.CardId))
            {
                return;
            }
            Buffs.RemoveWhere(s => !s.Card.CardId.Equals(indicator.Card.CardId), out ConcurrentBag<Buff.Buff> buffs);
            Buffs = buffs;
            //TODO: Find a better way to do this
            int randomTime = 0;
            if (Session is Character character)
            {
                if (indicator.Card.CardId == 85)
                {
                    randomTime = character.BuffRandomTime = ServerManager.Instance.RandomNumber(50, 350);
                }
                else if (indicator.Card.CardId == 336)
                {
                    randomTime = character.BuffRandomTime = ServerManager.Instance.RandomNumber(30, 70);
                }
                else if (indicator.Card.CardId == 0)
                {
                    character.BuffRandomTime = character.ChargeValue > 7000 ? 7000 : character.ChargeValue;
                }
                if (!indicator.StaticBuff)
                {
                    character.Session.SendPacket($"bf 1 {character.CharacterId} {(character.ChargeValue > 7000 ? 7000 : character.ChargeValue)}.{indicator.Card.CardId}.{indicator.Card.Duration} {Level}");
                    character.Session.SendPacket(character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("UNDER_EFFECT"), indicator.Card.Name), 20));
                }
            }
            if (!indicator.StaticBuff)
            {
                indicator.RemainingTime = indicator.Card.Duration == 0 ? randomTime : indicator.Card.Duration;
                indicator.Start = DateTime.Now;
            }
            Buffs.Add(indicator);
            indicator.Card.BCards.ForEach(c => c.ApplyBCards(Entity));

            if (indicator.Card.EffectId > 0)
            {
                //Entity.MapInstance?.Broadcast(Entity.GenerateEff(indicator.Card.EffectId));
            }
            if (ObservableBag.TryGetValue(indicator.Card.CardId, out IDisposable value))
            {
                value?.Dispose();
            }

            ObservableBag[indicator.Card.CardId] = Observable.Timer(TimeSpan.FromMilliseconds(indicator.StaticBuff ? indicator.RemainingTime * 1000 : indicator.RemainingTime * 100)).Subscribe(o =>
            {
                RemoveBuff(indicator.Card.CardId);
                if (indicator.Card.TimeoutBuff != 0 && ServerManager.Instance.RandomNumber() < indicator.Card.TimeoutBuffChance)
                {
                    AddBuff(new Buff.Buff(indicator.Card.TimeoutBuff, Level));
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        /// <param name="level"></param>
        public void DisableBuffs(List<BuffType> types, int level = 100)
        {
            lock (Buffs)
            {
                Buffs.Where(s => types.Contains(s.Card.BuffType) && !s.StaticBuff && s.Card.Level <= level).ToList()
                    .ForEach(s => RemoveBuff(s.Card.CardId));
            }
        }

        public ushort GenerateDamage(IBattleEntity targetEntity, Skill skill, ref int hitmode, ref bool onyxEffect)
        {
            BattleEntity target = targetEntity?.BattleEntity;
            if (target == null)
            {
                return 0;
            }

            #region Definitions

            // Percent Damage
            if (target.Session is MapMonster monster && monster.IsPercentage && monster.TakesDamage > 0)
            {
                return (ushort)monster.TakesDamage;
            }

            AttackType attackType = Entity.GetAttackType(skill);

            int morale = Level + GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0] - GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];
            short upgrade = AttackUpgrade;
            int critChance = Critical;
            int critHit = CriticalRate;
            int minDmg = MinDamage;
            int maxDmg = MaxDamage;
            int hitRate = HitRate;

            #endregion

            #region Get Weapon Stats

            if (Session is Character character)
            {
                if (skill == null)
                {
                    return 0;
                }
                if (skill.SkillVNum == 1085) // pas de bcard ...
                {
                    character.TeleportOnMap(targetEntity.GetPos().X, targetEntity.GetPos().Y);
                }
                if (character.Inventory.LoadBySlotAndType<WearableInstance>((byte)EquipmentType.Amulet, InventoryType.Equipment)?.Item?.Effect == 932)
                {
                    upgrade += 1;
                }
                DefenceUpgrade = character.Inventory?.Armor?.Upgrade ?? 0;

                if (CharacterHelper.Instance.GetClassAttackType(character.Class) == attackType)
                {
                    minDmg += character.MinHit;
                    maxDmg += character.MaxHit;
                    hitRate += character.HitRate;
                    critChance += character.HitCriticalRate;
                    critHit += character.HitCritical;
                    upgrade += character.Inventory.PrimaryWeapon?.Upgrade ?? 0;
                }
                else
                {
                    minDmg += character.MinDistance;
                    maxDmg += character.MaxDistance;
                    hitRate += character.DistanceRate;
                    critChance += character.DistanceCriticalRate;
                    critHit += character.DistanceCritical;
                    upgrade += character.Inventory.SecondaryWeapon?.Upgrade ?? 0;
                }
            }

            #endregion

            skill?.BCards?.ToList().ForEach(s => SkillBcards.Add(s));
            #region Switch skill.Type

            int targetDefence = target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.AllIncreased)[0]
                              - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.AllDecreased)[0];

            byte targetDefenseUpgrade = (byte)(target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelIncreased)[0]
                                             - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.DefenceLevelDecreased)[0]);

            int targetDodge = target.GetBuff(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased)[0]
                            - target.GetBuff(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased)[0];

            int targetMorale = target.Level + target.GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleIncreased)[0]
                                            - target.GetBuff(CardType.Morale, (byte)AdditionalTypes.Morale.MoraleDecreased)[0];

            int targetBoostpercentage = 0;

            int boost = GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.AllAttacksDecreased)[0];

            int boostpercentage = GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageIncreased)[0]
                                  - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.DamageDecreased)[0];

            switch (attackType)
            {
                case AttackType.Close:
                    targetDefence += target.CloseDefence;
                    targetDodge += target.DefenceRate;
                    targetBoostpercentage = target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeIncreased)[0]
                                          - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeDecreased)[0];
                    boost += GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0]
                           - GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeIncreased)[0]
                                     - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MeleeDecreased)[0];
                    break;

                case AttackType.Ranged:
                    targetDefence += target.RangedDefence;
                    targetDodge += target.DistanceDefenceRate;
                    targetBoostpercentage = target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.RangedIncreased)[0]
                                          - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.RangedDecreased)[0];
                    boost += GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksIncreased)[0]
                           - GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.RangedAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedIncreased)[0]
                                     - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.RangedDecreased)[0];
                    break;

                case AttackType.Magical:
                    targetDefence += target.MagicDefence;
                    targetBoostpercentage = target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MagicalIncreased)[0]
                                          - target.GetBuff(CardType.Defence, (byte)AdditionalTypes.Defence.MeleeDecreased)[0];
                    boost += GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksIncreased)[0]
                           - GetBuff(CardType.AttackPower, (byte)AdditionalTypes.AttackPower.MagicalAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalIncreased)[0]
                                     - GetBuff(CardType.Damage, (byte)AdditionalTypes.Damage.MagicalDecreased)[0];
                    break;
            }
            targetDefence = (int)(targetDefence * (1 + targetBoostpercentage / 100D));
            minDmg += boost;
            maxDmg += boost;
            minDmg = (int)(minDmg * (1 + boostpercentage / 100D));
            maxDmg = (int)(maxDmg * (1 + boostpercentage / 100D));

            #endregion

            upgrade -= target.DefenceUpgrade;

            #region Detailed Calculation

            #region Dodge

            if (attackType != AttackType.Magical)
            {
                double multiplier = targetDodge / (hitRate + 1);
                if (multiplier > 5)
                {
                    multiplier = 5;
                }
                double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier - 1.41;
                if (chance <= 1)
                {
                    chance = 1;
                }
                if (GetBuff(CardType.DodgeAndDefencePercent, (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased)[0] != 0)
                {
                    chance = 10;
                }
                if (skill?.Type == 0 || skill?.Type == 1)
                {
                    if (ServerManager.Instance.RandomNumber() <= chance)
                    {
                        hitmode = 1;
                        SkillBcards.Clear();
                        return 0;
                    }
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(minDmg, maxDmg < minDmg ? minDmg + 1 : maxDmg) + morale - targetMorale;

            double upgradeBonus = 0;
            switch (Math.Abs(upgrade))
            {
                case 1:
                    upgradeBonus = 0.1;
                    break;

                case 2:
                    upgradeBonus = 0.15;
                    break;

                case 3:
                    upgradeBonus = 0.22;
                    break;

                case 4:
                    upgradeBonus = 0.32;
                    break;

                case 5:
                    upgradeBonus = 0.43;
                    break;

                case 6:
                    upgradeBonus = 0.54;
                    break;

                case 7:
                    upgradeBonus = 0.65;
                    break;

                case 8:
                    upgradeBonus = 0.9;
                    break;

                case 9:
                    upgradeBonus = 1.2;
                    break;

                case 10:
                    upgradeBonus = 2;
                    break;
            }
            if (upgrade < 0)
            {
                targetDefence += (int)(targetDefence * upgradeBonus);
            }
            else
            {
                baseDamage += (int)(baseDamage * upgradeBonus);
            }

            baseDamage -= target.HasBuff(CardType.SpecialDefence, (byte)AdditionalTypes.SpecialDefence.AllDefenceNullified) ? 0 : targetDefence;

            if (skill?.Type == 1 && Map.Map.GetDistance(Entity.GetPos(), targetEntity.GetPos()) < 4)
            {
                baseDamage = (int)(baseDamage * 0.85);
            }

            #endregion

            #region Elementary Damage

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            int targetResistance = 0;
            int elementalDamage = GetBuff(CardType.Element, (byte)AdditionalTypes.Element.AllIncreased)[0] - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.AllDecreased)[0];
            int bonusrez = target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllIncreased)[0] - target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.AllDecreased)[0];

            switch (Element)
            {
                case 1:
                    bonusrez += target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireIncreased)[0]
                              - target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.FireDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.FireIncreased)[0]
                                     - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.FireDecreased)[0];
                    targetResistance = target.FireResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3; // Damage vs no element
                            break;

                        case 1:
                            elementalBoost = 1; // Damage vs fire
                            break;

                        case 2:
                            elementalBoost = 2; // Damage vs water
                            break;

                        case 3:
                            elementalBoost = 1; // Damage vs light
                            break;

                        case 4:
                            elementalBoost = 1.5; // Damage vs darkness
                            break;
                    }
                    break;

                case 2:
                    bonusrez += target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterIncreased)[0]
                              - target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.WaterDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.WaterIncreased)[0]
                                     - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.WaterDecreased)[0];
                    targetResistance = target.WaterResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 2;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1.5;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;

                case 3:
                    bonusrez += target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightIncreased)[0]
                              - target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.LightDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.LightIncreased)[0]
                                     - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.LightDecreased)[0];
                    targetResistance = target.LightResistance;
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1.5;
                            break;

                        case 2:
                            elementalBoost = 1;
                            break;

                        case 3:
                            elementalBoost = 1;
                            break;

                        case 4:
                            elementalBoost = 3;
                            break;
                    }
                    break;

                case 4:
                    bonusrez += target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkIncreased)[0]
                              - target.GetBuff(CardType.ElementResistance, (byte)AdditionalTypes.ElementResistance.DarkDecreased)[0];
                    targetResistance = target.DarkResistance;
                    elementalDamage += GetBuff(CardType.Element, (byte)AdditionalTypes.Element.DarkIncreased)[0]
                                     - GetBuff(CardType.Element, (byte)AdditionalTypes.Element.DarkDecreased)[0];
                    switch (target.Element)
                    {
                        case 0:
                            elementalBoost = 1.3;
                            break;

                        case 1:
                            elementalBoost = 1;
                            break;

                        case 2:
                            elementalBoost = 1.5;
                            break;

                        case 3:
                            elementalBoost = 3;
                            break;

                        case 4:
                            elementalBoost = 1;
                            break;
                    }
                    break;
            }

            #endregion;

            if (skill?.Element == 0)
            {
                switch (elementalBoost)
                {
                    case 0.5:
                        elementalBoost = 0;
                        break;

                    case 1:
                        elementalBoost = 0.05;
                        break;

                    case 1.3:
                    case 1.5:
                        elementalBoost = 0.15;
                        break;

                    case 2:
                    case 3:
                        elementalBoost = 0.2;
                        break;
                }
            }
            else if (skill?.Element != Element)
            {
                elementalBoost = 0;
            }

            elementalDamage = (int)(elementalDamage + (baseDamage + 100) * ((ElementRate + ElementRateSp) / 100D));
            elementalDamage = (int)(elementalDamage / 100D * (100 - targetResistance) * elementalBoost);

            #endregion

            #region Critical Damage

            critChance += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.InflictingIncreased)[0]
                              - GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.InflictingReduced)[0];

            critHit += GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreased)[0]
                           - GetBuff(CardType.Critical, (byte)AdditionalTypes.Critical.DamageIncreasedInflictingReduced)[0];

            if (ServerManager.Instance.RandomNumber() <= critChance)
            {
                if (skill?.Type != 2 && attackType != AttackType.Magical)
                {
                    double multiplier = critHit / 100D;
                    multiplier = multiplier > 3 ? 3 : multiplier;
                    baseDamage += (int)(baseDamage * multiplier);
                    hitmode = 3;
                }
            }


            #endregion

            // OFFENSIVE POTION
            baseDamage += (int)(baseDamage * GetBuff(CardType.Item, (byte)AdditionalTypes.Item.AttackIncreased)[0] / 100D);

            if (Session is Character charact)
            {
                int[] weaponSoftDamage = charact.GetWeaponSoftDamage();
                if (ServerManager.Instance.RandomNumber() < weaponSoftDamage[0])
                {
                    baseDamage += (int)(baseDamage * (1 + (weaponSoftDamage[1] / 100D)));
                    charact.MapInstance.Broadcast(charact.GenerateEff(15));
                }
                if (charact.ChargeValue > 0)
                {
                    baseDamage += charact.ChargeValue;
                    charact.ChargeValue = 0;
                    charact.RemoveBuff(0);
                }
                baseDamage += charact.Class == ClassType.Adventurer ? 20 : 0;
            }

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage;
            totalDamage = totalDamage < 5 ? ServerManager.Instance.RandomNumber(1, 6) : totalDamage;

            #endregion

            if (Session is MapMonster)
            {
                if (Level < 45)
                {
                    //no minimum damage
                }
                else if (Level < 55)
                {
                    totalDamage += Level;
                }
                else if (Level < 60)
                {
                    totalDamage += Level * 2;
                }
                else if (Level < 65)
                {
                    totalDamage += Level * 3;
                }
                else if (Level < 70)
                {
                    totalDamage += Level * 4;
                }
                else
                {
                    totalDamage += Level * 5;
                }
            }

            if (targetEntity.GetSession() is Character chara && target.HasBuff(CardType.NoDefeatAndNoDamage, (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower))
            {
                chara.ChargeValue = totalDamage;
                chara.AddBuff(new Buff.Buff(0));
                totalDamage = 0;
                hitmode = 1;
            }

            #endregion

            totalDamage = totalDamage > ushort.MaxValue ? ushort.MaxValue : totalDamage;

            #region Onyx Wings

            onyxEffect = GetBuff(CardType.StealBuff, (byte)AdditionalTypes.StealBuff.ChanceSummonOnyxDragon)[0] > ServerManager.Instance.RandomNumber();

            #endregion

            SkillBcards.Clear();

            return (ushort)(totalDamage > ushort.MaxValue ? ushort.MaxValue : totalDamage);
        }

        public int[] GetBuff(CardType type, byte subtype)
        {
            int value1 = 0;
            int value2 = 0;

            foreach (BCard entry in StaticBcards.Concat(SkillBcards).Where(s => s != null && s.Type.Equals((byte)type) && s.SubType.Equals(subtype)))
            {
                value1 += entry.IsLevelScaled ? entry.IsLevelDivided ? Level / entry.FirstData : entry.FirstData * Level : entry.FirstData;
                value2 += entry.SecondData;
            }

            foreach (Buff.Buff buff in Buffs)
            {
                foreach (BCard entry in buff.Card.BCards.Where(s => s.Type.Equals((byte)type) && s.SubType.Equals(subtype) &&
                    (s.CastType != 1 || s.CastType == 1 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now)))
                {
                    value1 += entry.IsLevelScaled ? entry.IsLevelDivided ? buff.Level / entry.FirstData : entry.FirstData * buff.Level : entry.FirstData;
                    value2 += entry.SecondData;
                }
            }

            return new[] { value1, value2 };
        }

        public bool HasBuff(CardType type, byte subtype)
        {
            return Buffs.Any(buff => buff.Card.BCards.Any(b => b.Type == (byte)type && b.SubType == subtype && (b.CastType != 1 || b.CastType == 1 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now))) ||
                   StaticBcards.Any(s => s.Type.Equals((byte)type) && s.SubType.Equals(subtype));
        }

        public void RemoveBuff(int id, bool removePermaBuff = false)
        {
            Buff.Buff indicator = Buffs.FirstOrDefault(s => s.Card.CardId == id);
            if (indicator == null || !removePermaBuff && indicator.IsPermaBuff && indicator.RemainingTime <= 0)
            {
                return;
            }

            if (indicator.IsPermaBuff && !removePermaBuff)
            {
                AddBuff(indicator);
                return;
            }

            Buffs.RemoveWhere(s => s.Card.CardId != id, out ConcurrentBag<Buff.Buff> buffs);
            Buffs = buffs;
            if (!(Session is Character character))
            {
                return;
            }


            if (indicator.StaticBuff)
            {
                character.Session.SendPacket($"vb {indicator.Card.CardId} 0 {indicator.Card.Duration}");
                character.Session.SendPacket(character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_TERMINATED"), indicator.Card.Name), 11));
            }
            else
            {
                character.Session.SendPacket($"bf 1 {character.CharacterId} 0.{indicator.Card.CardId}.0 {Level}");
                character.Session.SendPacket(character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_TERMINATED"), indicator.Card.Name), 20));
            }

            if (indicator.Card.BCards.Any(s => s.Type == (byte)CardType.Move))
            {
                character.LoadSpeed();
                character.LastSpeedChange = DateTime.Now;
                character.Session.SendPacket(character.GenerateCond());
            }

            if (!indicator.Card.BCards.Any(s => s.Type == (byte)CardType.SpecialActions && s.SubType.Equals((byte)AdditionalTypes.SpecialActions.Hide)))
            {
                return;
            }

            character.Invisible = false;
            character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m => character.MapInstance?.Broadcast(m.GenerateIn()));
            character.MapInstance?.Broadcast(character.GenerateInvisible());
        }

        public void TargetHit(IBattleEntity target, TargetHitType hitType, Skill skill, short? skillEffect = null, short? mapX = null, short? mapY = null, ComboDTO skillCombo = null, bool showTargetAnimation = false, bool isPvp = false)
        {
            if (!target.IsTargetable(Entity.SessionType(), isPvp) || (target.Faction == Entity.Faction && ServerManager.Instance.Act4Maps.Any(m => m == Entity.MapInstance)))
            {
                if (Session is Character cha)
                {
                    cha.Session.SendPacket($"cancel 2 {target.GetId()}");
                }
                return;
            }

            MapInstance mapInstance = target.MapInstance;
            int hitmode = 0;
            bool onyxWings = false;
            ushort damage = GenerateDamage(target, skill, ref hitmode, ref onyxWings);

            if (Session is Character charact && onyxWings && mapInstance != null)
            {
                short onyxX = (short)(charact.PositionX + 2);
                short onyxY = (short)(charact.PositionY + 2);
                int onyxId = mapInstance.GetNextId();
                MapMonster onyx = new MapMonster
                {
                    MonsterVNum = 2371,
                    MapX = onyxX,
                    MapY = onyxY,
                    MapMonsterId = onyxId,
                    IsHostile = false,
                    IsMoving = false,
                    ShouldRespawn = false
                };
                mapInstance.Broadcast($"guri 31 1 {charact.CharacterId} {onyxX} {onyxY}");
                onyx.Initialize(mapInstance);
                mapInstance.AddMonster(onyx);
                mapInstance.Broadcast(onyx.GenerateIn());
                target.GetDamage(damage / 2, false);
                Observable.Timer(TimeSpan.FromMilliseconds(350)).Subscribe(o =>
                {
                    mapInstance.Broadcast($"su {(byte)Entity.SessionType()} {onyxId} {(byte)target.SessionType()} {target.GetId()} -1 0 -1 {skill.Effect} -1 -1 1 92 {damage / 2} 0 0");
                    mapInstance.RemoveMonster(onyx);
                    mapInstance.Broadcast(onyx.GenerateOut());
                });
            }

            if (target.GetSession() is Character character)
            {
                damage = (ushort)(character.HasGodMode ? 0 : damage);
                if (character.IsSitting)
                {
                    character.IsSitting = false;
                    character.MapInstance.Broadcast(character.GenerateRest());
                }
            }
            else if (target.GetSession() is Mate mate)
            {
                if (mate.IsSitting)
                {
                    mate.IsSitting = false;
                    mate.Owner.MapInstance.Broadcast(mate.GenerateRest());
                }
            }

            int castTime = 0;
            if (skill != null && skill.CastEffect != 0)
            {
                Entity.MapInstance.Broadcast(Entity.GenerateEff(skill.CastEffect), Entity.GetPos().X, Entity.GetPos().Y);
                castTime = skill.CastTime * 100;
            }
            Observable.Timer(TimeSpan.FromMilliseconds(castTime)).Subscribe(o => TargetHit2(target, hitType, skill, damage, hitmode, skillEffect, mapX, mapY, skillCombo, showTargetAnimation, isPvp));
        }

        private void TargetHit2(IBattleEntity target, TargetHitType hitType, Skill skill, int damage, int hitmode, short? skillEffect = null, short? mapX = null, short? mapY = null, ComboDTO skillCombo = null, bool showTargetAnimation = false, bool isPvp = false, bool isRange = false)
        {
            target.GetDamage(damage, !(Session is MapMonster mon && mon.IsInvicible));
            string str = $"su {(byte)Entity.SessionType()} {Entity.GetId()} {(byte)target.SessionType()} {target.GetId()} {skill?.SkillVNum ?? 0} {skill?.Cooldown ?? 0}";
            switch (hitType)
            {
                case TargetHitType.SingleTargetHit:
                    str += $" {skill?.AttackAnimation ?? 11} {skill?.Effect ?? skillEffect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {damage} {hitmode} {skill?.SkillType - 1 ?? 0}";
                    break;

                case TargetHitType.SingleTargetHitCombo:
                    str += $" {skillCombo?.Animation ?? 0} {skillCombo?.Effect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {damage} {hitmode} {skill.SkillType - 1}";
                    break;

                case TargetHitType.SingleAOETargetHit:
                    switch (hitmode)
                    {
                        case 1:
                            hitmode = 4;
                            break;

                        case 3:
                            hitmode = 6;
                            break;

                        default:
                            hitmode = 5;
                            break;
                    }
                    if (showTargetAnimation)
                    {
                        Entity.MapInstance.Broadcast($" {skill?.AttackAnimation ?? 0} {skill?.Effect ?? 0} 0 0 {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} 0 0 {skill.SkillType - 1}");
                    }
                    str += $" {skill?.AttackAnimation ?? 0} {skill?.Effect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {damage} {hitmode} {skill.SkillType - 1}";
                    break;

                case TargetHitType.AOETargetHit:
                    switch (hitmode)
                    {
                        case 1:
                            hitmode = 4;
                            break;

                        case 3:
                            hitmode = 6;
                            break;

                        default:
                            hitmode = 5;
                            break;
                    }
                    str += $" {skill?.AttackAnimation ?? 0} {skill?.Effect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {damage} {hitmode} {skill.SkillType - 1}";
                    break;

                case TargetHitType.ZoneHit:
                    str += $" {skill?.AttackAnimation ?? 0} {skillEffect ?? 0} {mapX ?? Entity.GetPos().X} {mapY ?? Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {damage} 5 {skill.SkillType - 1}";
                    break;

                case TargetHitType.SpecialZoneHit:
                    str += $" {skill?.AttackAnimation ?? 0} {skillEffect ?? 0} {Entity.GetPos().X} {Entity.GetPos().Y} {(target.CurrentHp > 0 ? 1 : 0)} {(int)(target.CurrentHp / (double)target.MaxHp * 100)} {damage} 0 {skill.SkillType - 1}";
                    break;
            }
            Entity.MapInstance.Broadcast(str);

            bool isBoss = false;

            if (Entity.GetSession() is Character character)
            {
                character.LastSkillUse = DateTime.Now;
                RemoveBuff(85); // Hideout
            }
            else if (Entity.GetSession() is Mate mate)
            {
                mate.LastSkillUse = DateTime.Now;
            }
            if (target.GetSession() is MapMonster monster)
            {
                if (monster.Target == null)
                {
                    monster.LastSkill = DateTime.Now;
                }
                monster.Target = Entity;
                isBoss = monster.IsBoss;
                if (isBoss)
                {
                    Entity.MapInstance?.Broadcast(monster.GenerateBoss());
                }
                monster.DamageList[Entity] += damage;
            }

            if (!isBoss && skill != null)
            {
                foreach (BCard bcard in skill.BCards.Where(b => b != null))
                {
                    switch ((CardType)bcard.Type)
                    {
                        case CardType.Buff:
                            Buff.Buff b = new Buff.Buff(bcard.SecondData);
                            switch (b.Card?.BuffType)
                            {
                                case BuffType.Bad:
                                    bcard.ApplyBCards(target, Entity);
                                    break;

                                case BuffType.Good:
                                case BuffType.Neutral:
                                    bcard.ApplyBCards(Entity, Entity);
                                    break;
                            }
                            break;

                        case CardType.HealingBurningAndCasting:
                            switch ((AdditionalTypes.HealingBurningAndCasting)bcard.SubType)
                            {
                                case AdditionalTypes.HealingBurningAndCasting.RestoreHP:
                                case AdditionalTypes.HealingBurningAndCasting.RestoreHPWhenCasting:
                                    bcard.ApplyBCards(Entity, Entity);
                                    break;

                                default:
                                    bcard.ApplyBCards(target, Entity);
                                    break;
                            }
                            break;

                        default:
                            bcard.ApplyBCards(target, Entity);
                            break;
                    }
                }
            }
            

            if (target.CurrentHp <= 0)
            {
                target.GenerateDeath(Entity);
                Entity.GenerateRewards(target);
            }

            if (skill == null || (skill.Range <= 0 && skill.TargetRange <= 0) || isRange || !(Entity.GetSession() is MapMonster))
            {
                return;
            }
            
            foreach (IBattleEntity entitiesInRange in Entity.MapInstance?.GetBattleEntitiesInRange(Entity.GetPos(), skill.TargetRange).Where(e => e != target && e.IsTargetable(Entity.SessionType())))
            {
                TargetHit2(entitiesInRange, TargetHitType.SingleTargetHit, skill, damage, hitmode, isRange: true);
            }
        }

        #endregion
    }
}
