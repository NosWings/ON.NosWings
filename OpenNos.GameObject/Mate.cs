/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenNos.Domain.BCardType;
using System.Collections.Concurrent;
using OpenNos.Core;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class Mate : MateDTO
    {
        #region Members

        private NpcMonster _monster;

        private Character _owner;

        #endregion

        #region Instantiation

        public Mate()
        {
            Buff = new ConcurrentBag<Buff>();
        }

        public Mate(Character owner, NpcMonster npcMonster, byte level, MateType matetype)
        {
            NpcMonsterVNum = npcMonster.NpcMonsterVNum;
            Monster = npcMonster;
            Level = level;
            Hp = MaxHp;
            Mp = MaxMp;
            Name = npcMonster.Name;
            MateType = matetype;
            Loyalty = 1000;
            PositionY = (short)(owner.PositionY + 1);
            PositionX = (short)(owner.PositionX + 1);
            MapX = (short)(owner.PositionX + 1);
            MapY = (short)(owner.PositionY + 1);
            Direction = 2;
            CharacterId = owner.CharacterId;
            Owner = owner;
            GeneateMateTransportId();
            Buff = new ConcurrentBag<Buff>();
        }

        #endregion

        #region Properties

        public ItemInstance ArmorInstance { get; set; }

        public ItemInstance BootsInstance { get; set; }

        public ConcurrentBag<Buff> Buff { get; internal set; }

        public short CloseDefence { get; set; }

        public short Concentrate { get; set; }

        public byte CriticalChance { get; set; }

        public short CriticalRate { get; set; }

        public short DamageMaximum { get; set; }

        public short DamageMinimum { get; set; }

        public ItemInstance GlovesInstance { get; set; }

        public bool IsSitting { get; set; }

        public bool IsUsingSp { get; set; }

        public DateTime LastSpeedChange { get; set; }

        public short MagicDefence { get; set; }

        public int MateTransportId { get; set; }

        public int MaxHp { get { return HPLoad(); } }

        public int MaxMp { get { return MpLoad(); } }

        public NpcMonster Monster
        {
            get { return _monster ?? ServerManager.Instance.GetNpc(NpcMonsterVNum);}

            set { _monster = value;}
        }

        public Character Owner
        {
            get { return _owner ?? ServerManager.Instance.GetSessionByCharacterId(CharacterId).Character;}
            set { _owner = value;}
        }

        public byte PetId { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public Skill[] Skills
        {
            get
            {
                if (Skills == null)
                {
                    return new Skill[3];
                }
                return Skills;
            }
            set
            {
                Skills = value;                
            }
        }

        public byte Speed
        {
            get
            {
                byte bonusSpeed = (byte)(GetBuff(CardType.Move, (byte)AdditionalTypes.Move.SetMovementNegated)[0]
                                       + GetBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementSpeedIncreased)[0]
                                       + GetBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementSpeedDecreased)[0]);

                if (Monster.Speed + bonusSpeed > 59)
                {
                    return 59;
                }
                return (byte)(Monster.Speed + bonusSpeed);
            }

            set
            {
                LastSpeedChange = DateTime.Now;
                Monster.Speed = value > 59 ? (byte)59 : value;
            }
        }

        public ItemInstance SpInstance { get; set; }

        public ItemInstance WeaponInstance { get; set; }

        #endregion

        #region Methods

        public void GeneateMateTransportId()
        {
            int nextId = ServerManager.Instance.MateIds.Any() ? ServerManager.Instance.MateIds.Last() + 1 : 2000000;
            ServerManager.Instance.MateIds.Add(nextId);
            MateTransportId = nextId;
        }

        public string GenerateCMode(short morphId)
        {
            return $"c_mode 2 {MateTransportId} {morphId} 0 0";
        }

        public string GenerateCond()
        {
            return $"cond 2 {MateTransportId} 0 0 {Speed}";
        }

        public EffectPacket GenerateEff(int effectid)
        {
            return new EffectPacket
            {
                EffectType = 2,
                CharacterId = MateTransportId,
                Id = effectid
            };
        }

        public string GenerateEInfo()
        {
            return $"e_info 10 {NpcMonsterVNum} {Level} {Monster.Element} {Monster.AttackClass} {Monster.ElementRate} {Monster.AttackUpgrade} {Monster.DamageMinimum} {Monster.DamageMaximum} {Monster.Concentrate} {Monster.CriticalChance} {Monster.CriticalRate} {Monster.DefenceUpgrade} {Monster.CloseDefence} {Monster.DefenceDodge} {Monster.DistanceDefence} {Monster.DistanceDefenceDodge} {Monster.MagicDefence} {Monster.FireResistance} {Monster.WaterResistance} {Monster.LightResistance} {Monster.DarkResistance} {Monster.MaxHP} {Monster.MaxMP} -1 {Name.Replace(' ', '^')}";
        }

        public string GenerateIn(bool foe = false, bool isAct4 = false)
        {
            string name = Name.Replace(' ', '^');
            if (foe)
            {
                name = "!§$%&/()=?*+~#";
            }
            int faction = 0;
            if (isAct4)
            {
                faction = (byte)Owner.Faction + 2;
            }
            return $"in 2 {NpcMonsterVNum} {MateTransportId} {(IsTeamMember ? PositionX : MapX)} {(IsTeamMember ? PositionY : MapY)} {Direction} {(int)(Hp / (float)MaxHp * 100)} {(int)(Mp / (float)MaxMp * 100)} 0 {faction} 3 {CharacterId} 1 0 {(IsUsingSp && SpInstance != null ? SpInstance.Item.Morph : (Skin != 0 ? Skin : -1))} {name} 0 -1 0 0 0 0 0 0 0 0";
        }

        public string GenerateOut()
        {
            return $"out 2 {MateTransportId}";
        }

        public string GenerateRest()
        {
            IsSitting = !IsSitting;
            return $"rest 2 {MateTransportId} {(IsSitting ? 1 : 0)}";
        }

        public string GenerateSay(string message, int type)
        {
            return $"say 2 {MateTransportId} 2 {message}";
        }

        public string GenerateScPacket()
        {
            switch (MateType)
            {
                case MateType.Partner:
                    return $"sc_n {PetId} {NpcMonsterVNum} {MateTransportId} {Level} {Loyalty} {Experience} {(WeaponInstance != null ? $"{WeaponInstance.ItemVNum}.{WeaponInstance.Rare}.{WeaponInstance.Upgrade}" : "-1")} {(ArmorInstance != null ? $"{ArmorInstance.ItemVNum}.{ArmorInstance.Rare}.{ArmorInstance.Upgrade}" : "-1")} {(GlovesInstance != null ? $"{GlovesInstance.ItemVNum}.0.0" : "-1")} {(BootsInstance != null ? $"{BootsInstance.ItemVNum}.0.0" : "-1")} 0 0 1 0 142 174 232 4 70 0 73 158 86 158 69 0 0 0 0 0 2641 2641 1065 1065 0 285816 {(SpInstance != null ? "SP_NAME" : Name.Replace(' ', '^'))} {SpInstance?.Item.Morph ?? (Skin != 0 ? Skin : -1)} {(IsSummonable ? 1 : 0)} {(SpInstance != null ? $"{SpInstance.ItemVNum}.100" : "-1" )} -1 -1 -1";

                case MateType.Pet:
                    return $"sc_p {PetId} {NpcMonsterVNum} {MateTransportId} {Level} {Loyalty} {Experience} 0 {Monster.AttackUpgrade} {Monster.DamageMinimum} {Monster.DamageMaximum} {Monster.Concentrate} {Monster.CriticalChance} {Monster.CriticalRate} {Monster.DefenceUpgrade} {Monster.CloseDefence} {Monster.DefenceDodge} {Monster.DistanceDefence} {Monster.DistanceDefenceDodge} {Monster.MagicDefence} {Monster.Element} {Monster.FireResistance} {Monster.WaterResistance} {Monster.LightResistance} {Monster.DarkResistance} {Hp} {MaxHp} {Mp} {MaxMp} 0 15 {(CanPickUp ? 1 : 0)} {Name.Replace(' ', '^')} {(IsSummonable ? 1 : 0)}";
            }
            return string.Empty;
        }

        public string GenerateStatInfo()
        {
            return $"st 2 {MateTransportId} {Level} {(int)((float)Hp / (float)MaxHp * 100)} {(int)((float)Mp / (float)MaxMp * 100)} {Hp} {Mp}{Buff.Aggregate(string.Empty, (current, buff) => current + $" {buff.Card.CardId}.{buff.Level}")}";
        }

        public void GenerateXp(int xp)
        {
            if (Level < ServerManager.Instance.MaxLevel)
            {
                Experience += xp;
                if (Experience >= XpLoad())
                {
                    if (Level + 1 < Owner.Level)
                    {
                        Experience = (long) (Experience - XpLoad());
                        Level++;
                        Hp = MaxHp;
                        Mp = MaxMp;
                        Owner.MapInstance?.Broadcast(GenerateEff(6), PositionX, PositionY);
                        Owner.MapInstance?.Broadcast(GenerateEff(198), PositionX, PositionY);
                    }
                }
            }
            ServerManager.Instance.GetSessionByCharacterId(Owner.CharacterId).SendPacket(GenerateScPacket());
        }

        public int HPLoad()
        {
            double multiplicator = 1.0;
            int hp = 0;

            multiplicator += GetBuff(CardType.BearSpirit, (byte) AdditionalTypes.BearSpirit.IncreaseMaximumHP)[0] / 100D;
            multiplicator += GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.IncreasesMaximumHP)[0] / 100D;
            hp += GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.MaximumHPIncreased)[0];
            hp -= GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.MaximumHPDecreased)[0];
            hp += GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.MaximumHPMPIncreased)[0];
            // Monster Bonus HP
            hp += (int) (Monster.MaxHP - MateHelper.Instance.HpData[Monster.Level]);

            return (int) ((MateHelper.Instance.HpData[Level] + hp) * multiplicator);
        }

        public int MpLoad()
        {
            int mp = 0;
            double multiplicator = 1.0;
            multiplicator += GetBuff(CardType.BearSpirit, (byte) AdditionalTypes.BearSpirit.IncreaseMaximumMP)[0] / 100D;
            multiplicator += GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.IncreasesMaximumMP)[0] / 100D;
            mp += GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.MaximumMPIncreased)[0];
            mp -= GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.MaximumHPDecreased)[0];
            mp += GetBuff(CardType.MaxHPMP, (byte) AdditionalTypes.MaxHPMP.MaximumHPMPIncreased)[0];
            // Monster Bonus MP
            mp += (int)(Monster.MaxMP - (Monster.Race == 0 ? MateHelper.Instance.PrimaryMpData[Monster.Level] : MateHelper.Instance.SecondaryMpData[Monster.Level]));

            return (int) (((Monster.Race == 0 ? MateHelper.Instance.PrimaryMpData[Level] : MateHelper.Instance.SecondaryMpData[Level]) + mp) * multiplicator);
        }

        private List<ItemInstance> GetInventory()
        {
            List<ItemInstance> items = new List<ItemInstance>();
            switch (PetId)
            {
                case 0:
                    items = Owner.Inventory.Select(s => s.Value).Where(s => s.Type == InventoryType.FirstPartnerInventory).ToList();
                    break;

                case 1:
                    items = Owner.Inventory.Select(s => s.Value).Where(s => s.Type == InventoryType.SecondPartnerInventory).ToList();
                    break;

                case 2:
                    items = Owner.Inventory.Select(s => s.Value).Where(s => s.Type == InventoryType.ThirdPartnerInventory).ToList();
                    break;
            }
            return items;
        }

        public override void Initialize()
        {
        }

        /// <summary>
        /// Checks if the current character is in range of the given position
        /// </summary>
        /// <param name="xCoordinate">The x coordinate of the object to check.</param>
        /// <param name="yCoordinate">The y coordinate of the object to check.</param>
        /// <param name="range">The range of the coordinates to be maximal distanced.</param>
        /// <returns>True if the object is in Range, False if not.</returns>
        public bool IsInRange(int xCoordinate, int yCoordinate, int range)
        {
            return Math.Abs(PositionX - xCoordinate) <= range && Math.Abs(PositionY - yCoordinate) <= range;
        }

        public void LoadInventory()
        {
            List<ItemInstance> inv = GetInventory();
            if (inv.Count == 0)
            {
                return;
            }
            WeaponInstance = inv.FirstOrDefault(s => s.Item.EquipmentSlot == EquipmentType.MainWeapon);
            ArmorInstance = inv.FirstOrDefault(s => s.Item.EquipmentSlot == EquipmentType.Armor);
            GlovesInstance = inv.FirstOrDefault(s => s.Item.EquipmentSlot == EquipmentType.Gloves);
            BootsInstance = inv.FirstOrDefault(s => s.Item.EquipmentSlot == EquipmentType.Boots);
            SpInstance = inv.FirstOrDefault(s => s.Item.EquipmentSlot == EquipmentType.Sp);
        }

        private double XpLoad()
        {
            try
            {
                return MateHelper.Instance.XpData[Level - 1];
            }
            catch
            {
                return 0;
            }
        }

        public int[] GetBuff(CardType type, byte subtype)
        {
            int value1 = 0;
            int value2 = 0;

            foreach (Buff buff in Buff.Where(s => s?.Card?.BCards != null))
            {
                foreach (BCard entry in buff.Card.BCards.Where(s =>
                    s.Type.Equals((byte)type) && s.SubType.Equals(subtype) &&
                    (s.CastType != 1 || s.CastType == 1 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now)))
                {
                    if (entry.IsLevelScaled)
                    {
                        if (entry.IsLevelDivided)
                        {
                            value1 += buff.Level / entry.FirstData;
                        }
                        else
                        {
                            value1 += entry.FirstData * buff.Level;
                        }
                    }
                    else
                    {
                        value1 += entry.FirstData;
                    }
                    value2 += entry.SecondData;
                }
            }
            return new[] { value1, value2 };
        }

        public void AddBuff(Buff indicator)
        {
            if (indicator?.Card == null)
            {
                return;
            }
            Buff = Buff.Where(s => !s.Card.CardId.Equals(indicator.Card.CardId));
            indicator.RemainingTime = indicator.Card.Duration;
            indicator.Start = DateTime.Now;
            Buff.Add(indicator);
            indicator.Card.BCards.ForEach(c => c.ApplyBCards(this));
            if (indicator.Card.EffectId > 0)
            {
                GenerateEff(indicator.Card.EffectId);
            }
            Observable.Timer(TimeSpan.FromMilliseconds(indicator.Card.Duration * 100)).Subscribe(o => { RemoveBuff(indicator.Card.CardId); });
        }

        private void RemoveBuff(int id)
        {
            Buff indicator = Buff.FirstOrDefault(s => s.Card.CardId == id);
            if (indicator == null)
            {
                return;
            }
            if (Buff.Contains(indicator))
            {
                Buff = Buff.Where(s => s.Card.CardId != id);
            }
        }
        #endregion
    }
}