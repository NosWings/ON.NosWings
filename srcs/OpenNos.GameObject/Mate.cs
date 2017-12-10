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
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using static NosSharp.Enums.BCardType;
using System.Collections.Concurrent;
using OpenNos.Core;
using System.Reactive.Linq;
using NosSharp.Enums;
using OpenNos.Core.Extensions;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.GameObject.Packets.ServerPackets;
using OpenNos.PathFinder.PathFinder;
using OpenNos.GameObject.Battle;

namespace OpenNos.GameObject
{
    public class Mate : MateDTO, IBattleEntity
    {
        #region Members

        private NpcMonster _monster;

        private Character _owner;

        #endregion

        #region Instantiation

        public Mate()
        {
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
            MapX = ServerManager.Instance.MinilandRandomPos().X;
            MapY = ServerManager.Instance.MinilandRandomPos().Y;
            Direction = 2;
            CharacterId = owner.CharacterId;
            Owner = owner;
            GenerateMateTransportId();
        }

        #endregion

        #region Properties

        #region BattleEntityProperties

        public void AddBuff(Buff.Buff indicator) => GetBattleEntity().AddBuff(indicator);

        public void RemoveBuff(short cardId) => GetBattleEntity().RemoveBuff(cardId);

        public int[] GetBuff(CardType type, byte subtype) => GetBattleEntity().GetBuff(type, subtype);

        public bool HasBuff(CardType type, byte subtype) => GetBattleEntity().HasBuff(type, subtype);

        public ConcurrentBag<Buff.Buff> Buffs => GetBattleEntity().Buffs;

        #endregion

        public ItemInstance ArmorInstance { get; set; }

        public ItemInstance BootsInstance { get; set; }

        public Node[,] BrushFire { get; set; }

        public short CloseDefence { get; set; }

        public int Concentrate { get { return GetBattleEntity().HitRate; } set { GetBattleEntity().HitRate = value; } }

        public int DamageMaximum { get { return GetBattleEntity().MinDamage; } set { GetBattleEntity().MinDamage = value; } }

        public int DamageMinimum { get { return GetBattleEntity().MaxDamage; } set { GetBattleEntity().MaxDamage = value; } }

        public ItemInstance GlovesInstance { get; set; }

        public bool IsAlive { get; set; }

        public bool IsSitting { get; set; }

        public bool IsUsingSp { get; set; }

        public DateTime LastHealth { get; set; }

        public DateTime LastDeath { get; set; }

        public DateTime LastDefence { get; set; }

        public DateTime LastSpeedChange { get; set; }

        public DateTime LastSkillUse { get; set; }

        public IDisposable Life { get; private set; }

        public short MagicDefence { get; set; }

        public int MateTransportId { get; set; }

        public int MaxHp { get { return HpLoad(); } }

        public int MaxMp { get { return MpLoad(); } }

        public NpcMonster Monster
        {
            get { return _monster ?? (_monster = ServerManager.Instance.GetNpc(NpcMonsterVNum)); }

            set { _monster = value; }
        }

        public Character Owner
        {
            get { return _owner ?? ServerManager.Instance.GetSessionByCharacterId(CharacterId)?.Character; }
            set { _owner = value; }
        }

        public byte PetId { get; set; }

        public short PositionX { get; set; }

        public short PositionY { get; set; }

        public Skill[] Skills { get; set; }

        public ConcurrentBag<BCard> SkillBcards { get; set; }

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

        public void UpdateBushFire()
        {
            BrushFire = BestFirstSearch.LoadBrushFire(new GridPos()
            {
                X = PositionX,
                Y = PositionY
            }, Owner.MapInstance.Map.Grid);
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

        public void GenerateMateTransportId()
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
            return $"e_info 10 {NpcMonsterVNum} {Level} {Monster.Element} {Monster.AttackClass} {Monster.ElementRate} {Monster.AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {Monster.CriticalChance} {Monster.CriticalRate} {Monster.DefenceUpgrade} {Monster.CloseDefence} {Monster.DefenceDodge} {Monster.DistanceDefence} {Monster.DistanceDefenceDodge} {Monster.MagicDefence} {Monster.FireResistance} {Monster.WaterResistance} {Monster.LightResistance} {Monster.DarkResistance} {Monster.MaxHP} {Monster.MaxMP} -1 {Name.Replace(' ', '^')}";
        }

        public void GenerateDeath()
        {
            LastDeath = DateTime.Now;
            IsAlive = false;
            Hp = 1;
            Owner.Session.SendPacket(GenerateScPacket());
            Loyalty -= (short) (Owner.Authority >= AuthorityType.VipPlus ? 0 : 50);
            Owner.Session.SendPacket(GenerateScPacket());
            if (MateType == MateType.Pet)
            {
                if (Owner.IsPetAutoRelive)
                {
                    if (Owner.Inventory.CountItem(1012) < 5)
                    {
                        Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_REQUIERED_ITEM"), ServerManager.Instance.GetItem(1012).Name), 0));
                        Owner.IsPetAutoRelive = false;
                        BackToMiniland();
                        return;
                    }
                    Owner.Inventory.RemoveItemAmount(1012, 5);
                    Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("PET_WILL_BE_BACK"), 0));
                }
                else
                {
                    Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BACK_TO_MINILAND"), 0));
                    BackToMiniland();
                }
            }
            else if (MateType == MateType.Partner)
            {
                if (Owner.IsPartnerAutoRelive)
                {
                    if (Owner.Inventory.CountItem(1012) < 5)
                    {
                        Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_REQUIERED_ITEM"), ServerManager.Instance.GetItem(1012).Name), 0));
                        Owner.IsPartnerAutoRelive = false;
                        BackToMiniland();
                        return;
                    }
                    Owner.Inventory.RemoveItemAmount(1012, 5);
                    Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MATE_WILL_BE_BACK"), 0));
                }
                else
                {
                    Owner.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BACK_TO_MINILAND"), 0));
                    BackToMiniland();
                }
            }
        }

        public string GenerateIn(bool foe = false, bool isAct4 = false)
        {
            if (_owner.Invisible || _owner.InvisibleGm || !IsAlive)
            {
                return ""; //Maybe have to implement the exception on each mate.GenerateIn call.
            }
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

        private void GenerateLevelXpLevelUp()
        {
            double t = XpLoad();
            while (Experience >= t)
            {
                Experience -= (long)t;
                Level++;
                t = XpLoad();
                if (Level >= ServerManager.Instance.MaxMateLevel)
                {
                    Level = ServerManager.Instance.MaxMateLevel;
                    Experience = 0;
                }
                GetBattleEntity().Level = Level;
                RefreshStats();
                Hp = MaxHp;
                Mp = MaxMp;
                Owner.MapInstance?.Broadcast(GenerateEff(6), PositionX, PositionY);
                Owner.MapInstance?.Broadcast(GenerateEff(198), PositionX, PositionY);
            }
        }

        public string GenerateOut()
        {
            return $"out 2 {MateTransportId}";
        }

        public string GeneratePst()
        {
            return
                $"pst 2 {MateTransportId} {(int)MateType} {(int)(Hp / (float)MaxHp * 100)} {(int)(Mp / (float)MaxMp * 100)} {Hp} {Mp} 0 0 0";
        }

        public string GeneratePski()
        {
            if (Skills?.Length >= 3)
            {
                return $"pski {Skills?[0].SkillVNum} {Skills?[1].SkillVNum} {Skills?[2].SkillVNum}";
            }
            return "pski";
        }

        public string GenerateRc(int heal)
        {
            return $"rc 2 {MateTransportId} {heal} 0";
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
                    return $"sc_n {PetId} {NpcMonsterVNum} {MateTransportId} {Level} {Loyalty} {Experience} {(WeaponInstance != null ? $"{WeaponInstance.ItemVNum}.{WeaponInstance.Rare}.{WeaponInstance.Upgrade}" : "-1")} {(ArmorInstance != null ? $"{ArmorInstance.ItemVNum}.{ArmorInstance.Rare}.{ArmorInstance.Upgrade}" : "-1")} {(GlovesInstance != null ? $"{GlovesInstance.ItemVNum}.0.0" : "-1")} {(BootsInstance != null ? $"{BootsInstance.ItemVNum}.0.0" : "-1")} 0 0 1 0 142 174 232 4 70 0 73 158 86 158 69 0 0 0 0 0 {Hp} {MaxHp} {Mp} {MaxMp} 0 285816 {Name.Replace(' ', '^')} {(IsUsingSp && SpInstance != null ? SpInstance.Item.Morph : Skin != 0 ? Skin : -1)} {(IsSummonable ? 1 : 0)} {(SpInstance != null ? $"{SpInstance.ItemVNum}.100" : "-1")} -1 -1 -1";

                case MateType.Pet:
                    return $"sc_p {PetId} {NpcMonsterVNum} {MateTransportId} {Level} {Loyalty} {Experience} 0 {Monster.AttackUpgrade} {DamageMinimum} {DamageMaximum} {Concentrate} {Monster.CriticalChance} {Monster.CriticalRate} {Monster.DefenceUpgrade} {Monster.CloseDefence} {Monster.DefenceDodge} {Monster.DistanceDefence} {Monster.DistanceDefenceDodge} {Monster.MagicDefence} {Monster.Element} {Monster.FireResistance} {Monster.WaterResistance} {Monster.LightResistance} {Monster.DarkResistance} {Hp} {MaxHp} {Mp} {MaxMp} {(byte)(IsTeamMember ? 1 : 0)} {XpLoad()} {(byte)(CanPickUp ? 1 : 0)} {Name.Replace(' ', '^')} {(byte)(IsSummonable ? 1 : 0)}";
            }
            return string.Empty;
        }

        public string GenerateStatInfo()
        {
            return $"st 2 {MateTransportId} {Level} 0 {(int)(Hp / (float)MaxHp * 100)} {(int)(Mp / (float)MaxMp * 100)} {Hp} {Mp}";
        }

        public void GenerateXp(int xp)
        {
            if (Level < ServerManager.Instance.MaxMateLevel && Level < Owner.Level)
            {
                Experience += xp;
                GenerateLevelXpLevelUp();
            }
            Owner.Session.SendPacket(GenerateScPacket());
        }

        public void GetDamage(int damage, bool canKill = true)
        {
            if (Hp <= 0)
            {
                return;
            }
            LastDefence = DateTime.Now;
            Hp -= damage;
            if (Hp < 0)
            {
                Hp = 0;
            }
            if (!canKill && Hp == 0)
            {
                Hp = 1;
            }
        }

        private int HealthHpLoad()
        {
            int regen = GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryIncreased)[0];
            regen -= GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.HPRecoveryDecreased)[0];
            if (IsSitting)
            {
                return regen + 50;
            }
            return (DateTime.Now - LastDefence).TotalSeconds > 4 ? regen + 20 : 0;
        }

        private int HealthMpLoad()
        {
            int regen = GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryIncreased)[0];
            regen -= GetBuff(CardType.Recovery, (byte)AdditionalTypes.Recovery.MPRecoveryDecreased)[0];
            if (IsSitting)
            {
                return regen + 50;
            }
            return (DateTime.Now - LastDefence).TotalSeconds > 4 ? regen + 20 : 0;
        }

        public int HpLoad()
        {
            double multiplicator = 1.0;
            int hp = 0;

            multiplicator += GetBuff(CardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumHP)[0] / 100D;
            multiplicator += GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.IncreasesMaximumHP)[0] / 100D;
            hp += GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPIncreased)[0];
            hp -= GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPDecreased)[0];
            hp += GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPMPIncreased)[0];
            // Monster Bonus HP
            hp += Monster.MaxHP - MateHelper.Instance.HpData[Monster.Level];

            return (int)((MateHelper.Instance.HpData[Level] + hp) * multiplicator);
        }

        public override void Initialize()
        {
            byte type = (byte)(Monster.AttackClass == 2 ? 1 : 0);
            Concentrate = (short)(MateHelper.Instance.Concentrate[type, Level] + (Monster.Concentrate - MateHelper.Instance.Concentrate[type, Monster.Level]));
            DamageMinimum = (short)(MateHelper.Instance.MinDamageData[type, Level] + (Monster.DamageMinimum - MateHelper.Instance.MinDamageData[type, Monster.Level]));
            DamageMaximum = (short)(MateHelper.Instance.MaxDamageData[type, Level] + (Monster.DamageMaximum - MateHelper.Instance.MaxDamageData[type, Monster.Level]));
            IsAlive = true;
            Hp = MaxHp;
            Life = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(s =>
            {
                MateLife();
            });
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

        public int MpLoad()
        {
            int mp = 0;
            double multiplicator = 1.0;
            multiplicator += GetBuff(CardType.BearSpirit, (byte)AdditionalTypes.BearSpirit.IncreaseMaximumMP)[0] / 100D;
            multiplicator += GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.IncreasesMaximumMP)[0] / 100D;
            mp += GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumMPIncreased)[0];
            mp -= GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPDecreased)[0];
            mp += GetBuff(CardType.MaxHPMP, (byte)AdditionalTypes.MaxHPMP.MaximumHPMPIncreased)[0];
            // Monster Bonus MP
            mp += Monster.MaxMP - (Monster.Race == 0 ? MateHelper.Instance.PrimaryMpData[Monster.Level] : MateHelper.Instance.SecondaryMpData[Monster.Level]);

            return (int)(((Monster.Race == 0 ? MateHelper.Instance.PrimaryMpData[Level] : MateHelper.Instance.SecondaryMpData[Level]) + mp) * multiplicator);
        }

        private void MateLife()
        {
            if (!IsTeamMember)
            {
                return;
            }

            if (!IsAlive && LastDeath.AddMinutes(3) < DateTime.Now)
            {
                GenerateRevive();
            }
            if ((LastHealth.AddSeconds(2) <= DateTime.Now || IsSitting && LastHealth.AddSeconds(1.5) <= DateTime.Now) && IsAlive)
            {
                LastHealth = DateTime.Now;
                if (LastDefence.AddSeconds(4) <= DateTime.Now && LastSkillUse.AddSeconds(2) <= DateTime.Now && Hp > 0)
                {
                    if (Hp + HealthHpLoad() < HpLoad())
                    {
                        Hp += HealthHpLoad();
                    }
                    else
                    {
                        Hp = HpLoad();
                    }

                    if (Mp + HealthMpLoad() < MpLoad())
                    {
                        Mp += HealthMpLoad();
                    }
                    else
                    {
                        Mp = MpLoad();
                    }
                }
            }
            Owner?.Session?.SendPacket(GeneratePst());
        }

        public void BackToMiniland()
        {
            if (!IsTeamMember)
            {
                return;
            }
            IsTeamMember = false;
            IsAlive = true;
            Hp = MaxHp;
            Mp = MaxMp;
            Owner.Session.SendPacket(Owner.GeneratePinit());
            Owner.MapInstance.Broadcast(GenerateOut());
            MapX = ServerManager.Instance.MinilandRandomPos().X;
            MapY = ServerManager.Instance.MinilandRandomPos().Y;
        }

        public void GenerateRevive()
        {
            if (Owner == null)
            {
                return;
            }
            Owner.MapInstance?.Broadcast(GenerateOut());
            IsAlive = true;
            PositionY = (short)(Owner.PositionY + 1);
            PositionX = (short)(Owner.PositionX + 1);
            Owner.MapInstance?.Broadcast(GenerateIn());
            Owner.Session.SendPacket(GenerateCond());
            Owner.Session.SendPacket(Owner.GeneratePinit());
            Hp = MaxHp;
            Mp = MaxMp;
        }

        public void RefreshStats()
        {
            byte type = (byte)(Monster.AttackClass == 2 ? 1 : 0);
            Concentrate = (short)(MateHelper.Instance.Concentrate[type, Level] + (Monster.Concentrate - MateHelper.Instance.Concentrate[type, Monster.Level]));
            DamageMinimum = (short)(MateHelper.Instance.MinDamageData[type, Level] + (Monster.DamageMinimum - MateHelper.Instance.MinDamageData[type, Monster.Level]));
            DamageMaximum = (short)(MateHelper.Instance.MaxDamageData[type, Level] + (Monster.DamageMaximum - MateHelper.Instance.MaxDamageData[type, Monster.Level]));
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

        public MapCell GetPos()
        {
            return new MapCell { X = PositionX, Y = PositionY };
        }

        public BattleEntity GetBattleEntity()
        {
            return new BattleEntity(this);
        }

        public object GetSession()
        {
            return this;
        }

        public AttackType GetAttackType(Skill skill = null)
        {
            return (AttackType)Monster.AttackClass;
        }

        public bool isTargetable(SessionType type, bool isPvP = false)
        {
            return type == SessionType.Monster && IsAlive && Hp > 0;
        }

        public Node[,] GetBrushFire()
        {
            return BestFirstSearch.LoadBrushFire(new GridPos() { X = PositionX, Y = PositionY }, Owner.MapInstance.Map.Grid);
        }

        public SessionType GetSessionType()
        {
            return SessionType.Mate;
        }

        public long GetId()
        {
            return MateTransportId;
        }

        public MapInstance GetMapInstance()
        {
            return Owner.MapInstance;
        }

        public void GenerateDeath(IBattleEntity killer)
        {
            if (Hp > 0)
            {
                return;
            }
        }

        public void GenerateRewards(IBattleEntity target)
        {
            if (target is MapMonster monster)
            {
                Owner.GenerateKillBonus(monster);
            }
        }

        public int GetCurrentHp()
        {
            return Hp;
        }

        public int GetMaxHp()
        {
            return MaxHp;
        }

        #endregion
    }
}