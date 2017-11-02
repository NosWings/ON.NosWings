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

using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.PathFinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using static OpenNos.Domain.BCardType;

namespace OpenNos.GameObject
{
    public class MapMonster : MapMonsterDTO
    {
        #region Members

        private int _movetime;
        private Random _random;

        #endregion

        #region Instantiation

        public MapMonster()
        {
            HitQueue = new ConcurrentQueue<HitRequest>();
            OnDeathEvents = new List<EventContainer>();
            OnNoticeEvents = new List<EventContainer>();
        }

        #endregion

        #region Properties

        public ConcurrentBag<Buff> Buff { get; internal set; }

        public ConcurrentBag<BCard> SkillBcards { get; set; }

        public int CurrentHp { get; set; }

        public int CurrentMp { get; set; }

        public IDictionary<long, long> DamageList { get; private set; }

        public IDictionary<long, long> MatesDamageList { get; private set; }

        public DateTime Death { get; set; }

        public ConcurrentQueue<HitRequest> HitQueue { get; }

        public bool IsAlive { get; set; }

        public bool IsFactionTargettable(FactionType faction)
        {
            switch (MonsterVNum)
            {
                case 679:
                    if (faction == FactionType.Angel)
                    {
                        return false;
                    }
                    break;
                case 680:
                    if (faction == FactionType.Demon)
                    {
                        return false;
                    }
                    break;
            }
            return true;
        }

        public bool IsBonus { get; set; }

        public bool IsBoss { get; set; }

        public byte NoticeRange { get; set; }

        public bool IsHostile { get; set; }

        public bool IsTarget { get; set; }

        public DateTime LastEffect { get; set; }

        public DateTime LastMove { get; set; }

        public DateTime LastSkill { get; set; }

        public IDisposable LifeEvent { get; set; }

        public MapInstance MapInstance { get; set; }

        public NpcMonster Monster { get; private set; }

        public List<EventContainer> OnDeathEvents { get; set; }

        public List<EventContainer> OnNoticeEvents { get; set; }

        public ZoneEvent MoveEvent { get; set; }

        public List<Node> Path { get; set; }

        public bool? ShouldRespawn { get; set; }

        public ConcurrentBag<NpcMonsterSkill> Skills { get; set; } = new ConcurrentBag<NpcMonsterSkill>();

        public bool Started { get; internal set; }

        public object Target { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public IDisposable Life { get; set; }

        #endregion

        #region Methods

        public EffectPacket GenerateEff(int effectid)
        {
            return new EffectPacket
            {
                EffectType = 3,
                CharacterId = MapMonsterId,
                Id = effectid
            };
        }

        public string GenerateIn()
        {
            if (IsAlive && !IsDisabled)
            {
                return
                    $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {(int) ((float) CurrentMp / (float) Monster.MaxMP * 100)} 0 0 0 -1 {(Monster.NoAggresiveIcon ? (byte) InRespawnType.NoEffect : (byte) InRespawnType.TeleportationEffect)} 0 -1 - 0 -1 0 0 0 0 0 0 0 0";
            }
            return string.Empty;
        }

        public string GenerateOut()
        {
            return $"out 3 {MapMonsterId}";
        }

        public string GenerateSay(string message, int type)
        {
            return $"say 3 {MapMonsterId} {type} {message}";
        }

        public void Initialize(MapInstance currentMapInstance)
        {
            MapInstance = currentMapInstance;
            Initialize();
            StartLife();
        }

        public override void Initialize()
        {
            FirstX = MapX;
            FirstY = MapY;
            Life = null;
            LastSkill = LastMove = LastEffect = DateTime.Now;
            Target = null;
            Path = new List<Node>();
            IsAlive = true;
            ShouldRespawn = ShouldRespawn ?? true;
            Monster = ServerManager.Instance.GetNpc(MonsterVNum);
            IsHostile = Monster.IsHostile;
            CurrentHp = Monster.MaxHP;
            CurrentMp = Monster.MaxMP;
            Monster.Skills.ForEach(s => Skills.Add(s));
            DamageList = new Dictionary<long, long>();
            MatesDamageList = new Dictionary<long, long>();
            _random = new Random(MapMonsterId);
            _movetime = ServerManager.Instance.RandomNumber(400, 3200);
            Buff = new ConcurrentBag<Buff>();
            SkillBcards = new ConcurrentBag<BCard>();
        }

        /// <summary>
        /// Check if the Monster is in the given Range.
        /// </summary>
        /// <param name="mapX">The X coordinate on the Map of the object to check.</param>
        /// <param name="mapY">The Y coordinate on the Map of the object to check.</param>
        /// <param name="distance">The maximum distance of the object to check.</param>
        /// <returns>True if the Monster is in range, False if not.</returns>
        public bool IsInRange(short mapX, short mapY, byte distance)
        {
            return Map.GetDistance(
                       new MapCell
                       {
                           X = mapX,
                           Y = mapY
                       }, new MapCell
                       {
                           X = MapX,
                           Y = MapY
                       }) <= distance + 1;
        }

        /// <summary>
        /// Run the Death Event
        /// </summary>
        public void RunDeathEvent()
        {
            if (IsBonus)
            {
                MapInstance.InstanceBag.Combo++;
                MapInstance.InstanceBag.Point +=
                    EventHelper.Instance.CalculateComboPoint(MapInstance.InstanceBag.Combo + 1);
            }
            else
            {
                MapInstance.InstanceBag.Combo = 0;
                MapInstance.InstanceBag.Point +=
                    EventHelper.Instance.CalculateComboPoint(MapInstance.InstanceBag.Combo);
            }
            MapInstance.InstanceBag.MonstersKilled++;
            OnDeathEvents.ForEach(e => { EventHelper.Instance.RunEvent(e, monster: this); });
        }

        /// <summary>
        /// Start life
        /// </summary>
        public void StartLife()
        {
            if (!MapInstance.IsSleeping && Life == null)
            {
                Life = Observable.Interval(TimeSpan.FromMilliseconds(400)).Subscribe(x =>
                {
                    try
                    {
                        if (!MapInstance.IsSleeping)
                        {
                            MonsterLife();
                        }
                        else
                        {
                            IDisposable tmp = Life;
                            Life = null;
                            tmp.Dispose();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                });
            }
        }

        /// <summary>
        /// Get the Nearest Oponent
        /// </summary>
        internal void GetNearestOponent()
        {
            if (Target != null)
            {
                return;
            }
            const int maxDistance = 22;
            int sessDistance = 100;
            int mateDistance = 100;
            List<ClientSession> sess = new List<ClientSession>();
            List<Mate> mates = new List<Mate>();
            MatesDamageList.Keys.ToList().ForEach(m => mates.Add(MapInstance.GetMateByMateTransportId(m)));
            DamageList.Keys.ToList().ForEach(s => sess.Add(MapInstance.GetSessionByCharacterId(s)));
            ClientSession session = sess.OrderBy(s => sessDistance = Map.GetDistance(new MapCell {X = MapX, Y = MapY},
                new MapCell {X = s.Character.PositionX, Y = s.Character.PositionY})).FirstOrDefault();
            Mate mate = mates.OrderBy(s => mateDistance = Map.GetDistance(new MapCell {X = MapX, Y = MapY},
                new MapCell {X = s.PositionX, Y = s.PositionY})).FirstOrDefault();
            if (mateDistance < sessDistance)
            {
                if (mateDistance >= maxDistance)
                {
                    return;
                }
                if (mate != null)
                {
                    Target = mate;
                }
            }
            else
            {
                if (sessDistance >= maxDistance)
                {
                    return;
                }
                if (session != null)
                {
                    Target = session.Character;
                }
            }
        }

        /// <summary>
        /// Hostility on actual Target
        /// </summary>
        internal void HostilityTarget()
        {
            if (!IsHostile || Target != null)
            {
                return;
            }
            Character character = MapInstance.Sessions.FirstOrDefault(s =>
                    s?.Character != null && s.Character.Hp > 0 && !s.Character.InvisibleGm && !s.Character.Invisible &&
                    s.Character.MapInstance == MapInstance &&
                    IsFactionTargettable(s.Character.Faction) &&
                    Map.GetDistance(new MapCell {X = MapX, Y = MapY},
                        new MapCell {X = s.Character.PositionX, Y = s.Character.PositionY}) <
                    (NoticeRange == 0 ? Monster.NoticeRange : NoticeRange))
                ?.Character;

            Mate mate = MapInstance.Mates.FirstOrDefault(m =>
                m.IsAlive && m.Hp > 0 &&
                Map.GetDistance(new MapCell {X = MapX, Y = MapY}, new MapCell {X = m.PositionX, Y = m.PositionY}) <
                (NoticeRange == 0 ? Monster.NoticeRange : NoticeRange));

            if (character == null && mate == null)
            {
                return;
            }

            int characterDistance = character != null
                ? Map.GetDistance(new MapCell {X = MapX, Y = MapY},
                    new MapCell {X = character.PositionX, Y = character.PositionY})
                : 500;
            int mateDistance = mate != null
                ? Map.GetDistance(new MapCell {X = MapX, Y = MapY},
                    new MapCell {X = mate.PositionX, Y = mate.PositionY})
                : 500;

            if (!OnNoticeEvents.Any() && MoveEvent == null)
            {
                Target = character == null ? mate : characterDistance < mateDistance ? character : (object) mate;
                if (!Monster.NoAggresiveIcon && LastEffect.AddSeconds(5) < DateTime.Now)
                {
                    character?.Session.SendPacket(GenerateEff(5000));
                }
            }
            OnNoticeEvents.ForEach(e => { EventHelper.Instance.RunEvent(e, monster: this); });
            OnNoticeEvents.RemoveAll(s => s != null);
        }

        /// <summary>
        /// Remove the current Target from Monster.
        /// </summary>
        internal void RemoveTarget()
        {
            if (Target == null)
            {
                return;
            }
            Path.Clear();
            Target = null;
            //return to origin
            Path = BestFirstSearch.FindPath(new Node {X = MapX, Y = MapY}, new Node {X = FirstX, Y = FirstY},
                MapInstance.Map.Grid);
        }

        /// <summary>
        /// Follow the Monsters target to it's position.
        /// </summary>
        /// <param name="targetSession">The TargetSession to follow</param>
        private void FollowTarget()
        {
            if (Monster == null || !IsAlive || HasBuff(CardType.Move, (byte) AdditionalTypes.Move.MovementImpossible) ||
                !IsMoving)
            {
                return;
            }

            if (Target == null)
            {
                RemoveTarget();
            }

            Node[,] brushFire = null;
            int mapId = 0;
            short mapX = 0;
            short mapY = 0;
            if (Target is Character character)
            {
                character.UpdateBushFire();
                brushFire = character.BrushFire;
                mapId = character.MapInstance.Map.MapId;
                mapX = character.PositionX;
                mapY = character.PositionY;
            }

            if (Target is Mate mate)
            {
                mate.UpdateBushFire();
                brushFire = mate.BrushFire;
                mapId = mate.Owner.MapInstance.Map.MapId;
                mapX = mate.PositionX;
                mapY = mate.PositionY;
            }

            if (MapId != mapId)
            {
                RemoveTarget();
            }
            if (!Path.Any() && brushFire != null)
            {
                try
                {
                    List<Node> list = BestFirstSearch.TracePath(new Node() {X = MapX, Y = MapY}, brushFire,
                        MapInstance.Map.Grid);
                    Path = list;
                }
                catch (Exception ex)
                {
                    Logger.Log.Error($"Pathfinding using Pathfinder failed. Map: {MapId} StartX: {MapX} StartY: {MapY}",
                        ex);
                    RemoveTarget();
                }
            }
            short maxDistance = 22;
            int distance = Map.GetDistance(new MapCell {X = mapX, Y = mapY}, new MapCell {X = MapX, Y = MapY});
            if (Monster != null && DateTime.Now > LastMove && Monster.Speed > 0 && Path.Any())
            {
                int maxindex = Path.Count > Monster.Speed / 2 ? Monster.Speed / 2 : Path.Count;
                short smapX = Path[maxindex - 1].X;
                short smapY = Path[maxindex - 1].Y;
                double waitingtime =
                    Map.GetDistance(new MapCell {X = smapX, Y = smapY}, new MapCell {X = MapX, Y = MapY}) /
                    (double) Monster.Speed;
                MapInstance.Broadcast(new BroadcastPacket(null, $"mv 3 {MapMonsterId} {smapX} {smapY} {Monster.Speed}",
                    ReceiverType.All, xCoordinate: smapX, yCoordinate: smapY));
                LastMove = DateTime.Now.AddSeconds(waitingtime > 1 ? 1 : waitingtime);

                Observable.Timer(TimeSpan.FromMilliseconds((int) ((waitingtime > 1 ? 1 : waitingtime) * 1000)))
                    .Subscribe(x =>
                    {
                        MapX = smapX;
                        MapY = smapY;
                    });
                distance = (int) Path[0].F;
                Path.RemoveRange(0, maxindex);
                if (distance > (maxDistance) + 3)
                {
                    RemoveTarget();
                }
            }
        }

        /// <summary>
        /// Generate the Monster -&gt; Character Damage
        /// </summary>
        /// <param name="targetCharacter">Target Character to Hit</param>
        /// <param name="skill">Skill that MapMonster is using</param>
        /// <param name="hitmode">Actual hitmode</param>
        /// <returns></returns>
        private int GenerateDamage(Character targetCharacter, Skill skill, ref int hitmode)
        {
            #region Definitions

            if (targetCharacter == null)
            {
                return 0;
            }

            int playerDefense =
                targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.AllIncreased)[0]
                - targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.AllDecreased)[0];

            byte playerDefenseUpgrade =
                (byte) (targetCharacter.GetBuff(CardType.Defence,
                            (byte) AdditionalTypes.Defence.DefenceLevelIncreased)[0]
                        - targetCharacter.GetBuff(CardType.Defence,
                            (byte) AdditionalTypes.Defence.DefenceLevelDecreased)[0]);

            int playerDodge = targetCharacter.GetBuff(CardType.DodgeAndDefencePercent,
                                  (byte) AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased)[0]
                              - targetCharacter.GetBuff(CardType.DodgeAndDefencePercent,
                                  (byte) AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased)[0];

            int playerMorale = targetCharacter.Level +
                               targetCharacter.GetBuff(CardType.Morale,
                                   (byte) AdditionalTypes.Morale.MoraleIncreased)[0]
                               - targetCharacter.GetBuff(CardType.Morale,
                                   (byte) AdditionalTypes.Morale.MoraleDecreased)[0];

            int morale = Monster.Level + GetBuff(CardType.Morale, (byte) AdditionalTypes.Morale.MoraleIncreased)[0]
                         - GetBuff(CardType.Morale, (byte) AdditionalTypes.Morale.MoraleDecreased)[0];

            if (targetCharacter.Inventory.Armor != null)
            {
                playerDefenseUpgrade += targetCharacter.Inventory.Armor.Upgrade;
            }

            short mainUpgrade = Monster.AttackUpgrade;
            int mainCritChance = Monster.CriticalChance;
            int mainCritHit = Monster.CriticalRate - 30;
            int mainMinDmg = Monster.DamageMinimum;
            int mainMaxDmg = Monster.DamageMaximum;
            int mainHitRate = Monster.Concentrate; //probably missnamed, check later
            if (mainMaxDmg == 0)
            {
                mainMinDmg = Monster.Level * 8;
                mainMaxDmg = Monster.Level * 12;
                mainCritChance = 10;
                mainCritHit = 120;
                mainHitRate = Monster.Level / 2 + 1;
            }

            #endregion

            #region Get Player defense

            skill?.BCards?.ToList().ForEach(s => SkillBcards.Add(s));

            int playerBoostpercentage;

            int boost = GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.AllAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.AllAttacksDecreased)[0];

            int boostpercentage = GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.DamageIncreased)[0]
                                  - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.DamageDecreased)[0];

            WearableInstance amulet =
                targetCharacter.Inventory.LoadBySlotAndType<WearableInstance>((byte) EquipmentType.Amulet,
                    InventoryType.Equipment);

            if (amulet != null && amulet.Item.Effect == 933)
            {
                playerDefenseUpgrade += 1;
            }

            switch (Monster.AttackClass)
            {
                case 0:
                    playerDefense += targetCharacter.Defence;
                    playerDodge += targetCharacter.DefenceRate;
                    playerBoostpercentage =
                        targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MeleeIncreased)[0]
                        - targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MeleeDecreased)[0];
                    playerDefense = (int) (playerDefense * (1 + playerBoostpercentage / 100D));

                    boost += GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0]
                             - GetBuff(CardType.AttackPower,
                                 (byte) AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MeleeIncreased)[0]
                                       - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MeleeDecreased)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int) (mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int) (mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 1:
                    playerDefense += targetCharacter.DistanceDefence;
                    playerDodge += targetCharacter.DistanceDefenceRate;
                    playerBoostpercentage =
                        targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.RangedIncreased)[0]
                        - targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.RangedDecreased)[0];
                    playerDefense = (int) (playerDefense * (1 + playerBoostpercentage / 100D));

                    boost += GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.RangedAttacksIncreased)[0]
                             - GetBuff(CardType.AttackPower,
                                 (byte) AdditionalTypes.AttackPower.RangedAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.RangedIncreased)[0]
                                       - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.RangedDecreased)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int) (mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int) (mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 2:
                    playerDefense += targetCharacter.MagicalDefence;
                    playerBoostpercentage =
                        targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MagicalIncreased)[0]
                        - targetCharacter.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MeleeDecreased)[0];
                    playerDefense = (int) (playerDefense * (1 + playerBoostpercentage / 100D));

                    boost +=
                        GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.MagicalAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.MagicalAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MagicalIncreased)[0]
                                       - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MagicalDecreased)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int) (mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int) (mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                default:
                    throw new Exception($"Monster.AttackClass {Monster.AttackClass} not implemented");
            }

            #endregion

            #region Basic Damage Data Calculation

            mainCritChance +=
                targetCharacter.GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.ReceivingIncreased)[0]
                + GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.InflictingIncreased)[0]
                - targetCharacter.GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.ReceivingDecreased)[0]
                - GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.InflictingReduced)[0];

            mainCritHit += GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.DamageIncreased)[0]
                           - GetBuff(CardType.Critical,
                               (byte) AdditionalTypes.Critical.DamageIncreasedInflictingReduced)[0];

            // Critical damage deacreased by x %
            mainCritHit = (int) ((mainCritHit / 100D) * (100 + targetCharacter.GetBuff(CardType.Critical,
                                                             (byte) AdditionalTypes.Critical
                                                                 .DamageFromCriticalIncreased)[0]
                                                         - targetCharacter.GetBuff(CardType.Critical,
                                                             (byte) AdditionalTypes.Critical
                                                                 .DamageFromCriticalDecreased)[0]));

            mainUpgrade -= playerDefenseUpgrade;

            #endregion

            #region Detailed Calculation

            #region Dodge

            double multiplier = playerDodge / (double) mainHitRate;
            if (multiplier > 5)
            {
                multiplier = 5;
            }
            double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier - 1.41;
            if (chance <= 1)
            {
                chance = 1;
            }
            if (Monster.AttackClass == 0 || Monster.AttackClass == 1)
            {
                if (ServerManager.Instance.RandomNumber() <= chance)
                {
                    hitmode = 1;
                    return 0;
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(mainMinDmg, mainMaxDmg + 1);
            baseDamage += morale - playerMorale;

            switch (mainUpgrade)
            {
                case -10:
                    playerDefense += playerDefense * 2;
                    break;

                case -9:
                    playerDefense += (int) (playerDefense * 1.2);
                    break;

                case -8:
                    playerDefense += (int) (playerDefense * 0.9);
                    break;

                case -7:
                    playerDefense += (int) (playerDefense * 0.65);
                    break;

                case -6:
                    playerDefense += (int) (playerDefense * 0.54);
                    break;

                case -5:
                    playerDefense += (int) (playerDefense * 0.43);
                    break;

                case -4:
                    playerDefense += (int) (playerDefense * 0.32);
                    break;

                case -3:
                    playerDefense += (int) (playerDefense * 0.22);
                    break;

                case -2:
                    playerDefense += (int) (playerDefense * 0.15);
                    break;

                case -1:
                    playerDefense += (int) (playerDefense * 0.1);
                    break;

                case 0:
                    break;

                case 1:
                    baseDamage += (int) (baseDamage * 0.1);
                    break;

                case 2:
                    baseDamage += (int) (baseDamage * 0.15);
                    break;

                case 3:
                    baseDamage += (int) (baseDamage * 0.22);
                    break;

                case 4:
                    baseDamage += (int) (baseDamage * 0.32);
                    break;

                case 5:
                    baseDamage += (int) (baseDamage * 0.43);
                    break;

                case 6:
                    baseDamage += (int) (baseDamage * 0.54);
                    break;

                case 7:
                    baseDamage += (int) (baseDamage * 0.65);
                    break;

                case 8:
                    baseDamage += (int) (baseDamage * 0.9);
                    break;

                case 9:
                    baseDamage += (int) (baseDamage * 1.2);
                    break;

                case 10:
                    baseDamage += baseDamage * 2;
                    break;
            }

            #endregion

            #region Elementary Damage

            int elementalDamage = GetBuff(CardType.Element, (byte) AdditionalTypes.Element.AllIncreased)[0] -
                                  GetBuff(CardType.Element, (byte) AdditionalTypes.Element.AllDecreased)[0];

            int bonusrez = targetCharacter.GetBuff(CardType.ElementResistance,
                               (byte) AdditionalTypes.ElementResistance.AllIncreased)[0]
                           - targetCharacter.GetBuff(CardType.ElementResistance,
                               (byte) AdditionalTypes.ElementResistance.AllDecreased)[0];

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            int playerRessistance = 0;
            switch (Monster.Element)
            {
                case 0:
                    break;

                case 1:
                    bonusrez += targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.FireIncreased)[0]
                                - targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.FireDecreased)[0];

                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.FireIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.FireDecreased)[0];

                    playerRessistance = targetCharacter.FireResistance;
                    switch (targetCharacter.Element)
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
                    bonusrez += targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.WaterIncreased)[0]
                                - targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.WaterDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.WaterIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.WaterDecreased)[0];
                    playerRessistance = targetCharacter.WaterResistance;
                    switch (targetCharacter.Element)
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
                    bonusrez += targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.LightIncreased)[0]
                                - targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.LightDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.LightIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.LightDecreased)[0];
                    playerRessistance = targetCharacter.LightResistance;
                    switch (targetCharacter.Element)
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
                    bonusrez += targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.DarkIncreased)[0]
                                - targetCharacter.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.DarkDecreased)[0];
                    playerRessistance = targetCharacter.DarkResistance;
                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.DarkIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.DarkDecreased)[0];
                    switch (targetCharacter.Element)
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

            if (Monster.Element == 0)
            {
                if (elementalBoost == 0.5)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1)
                {
                    elementalBoost = 0.05;
                }
                else if (elementalBoost == 1.3)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1.5)
                {
                    elementalBoost = 0.15;
                }
                else if (elementalBoost == 2)
                {
                    elementalBoost = 0.2;
                }
                else if (elementalBoost == 3)
                {
                    elementalBoost = 0.2;
                }
            }
            elementalDamage =
                (int) ((elementalDamage + (100 + baseDamage) * (Monster.ElementRate / 100D)) * elementalBoost);
            elementalDamage = elementalDamage / 100 * (100 - playerRessistance - bonusrez);
            if (elementalDamage < 0)
            {
                elementalDamage = 0;
            }

            #endregion

            #region Critical Damage

            if (ServerManager.Instance.RandomNumber() <= mainCritChance)
            {
                if (Monster.AttackClass == 2)
                {
                }
                else
                {
                    baseDamage += (int) (baseDamage * (mainCritHit / 100D));
                    hitmode = 3;
                }
            }

            SkillBcards.Clear();

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage -
                              (targetCharacter.HasBuff(CardType.SpecialDefence,
                                  (byte) AdditionalTypes.SpecialDefence.AllDefenceNullified)
                                  ? 0
                                  : playerDefense);
            if (totalDamage < 5)
            {
                totalDamage = ServerManager.Instance.RandomNumber(1, 6);
            }

            #endregion

            #endregion

            #region Minimum damage

            if (Monster.Level < 45)
            {
                //no minimum damage
            }
            else if (Monster.Level < 55)
            {
                totalDamage += Monster.Level;
            }
            else if (Monster.Level < 60)
            {
                totalDamage += Monster.Level * 2;
            }
            else if (Monster.Level < 65)
            {
                totalDamage += Monster.Level * 3;
            }
            else if (Monster.Level < 70)
            {
                totalDamage += Monster.Level * 4;
            }
            else
            {
                totalDamage += Monster.Level * 5;
            }

            #endregion

            if (Monster.NpcMonsterVNum == 2309)
            {
                totalDamage = (int) (targetCharacter.HpLoad() / 10);
            }
            if (Monster.NpcMonsterVNum == 1381)
            {
                totalDamage = (int) (targetCharacter.HpLoad() / 15);
            }

            return totalDamage;
        }

        /// <summary>
        /// Generate the Monster -&gt; Character Damage
        /// </summary>
        /// <param name="targetMate"></param>
        /// <param name="skill"></param>
        /// <param name="hitmode"></param>
        /// <returns></returns>
        /// 
        private int GenerateDamage(Mate targetMate, Skill skill, ref int hitmode)
        {
            #region Definitions

            if (targetMate == null)
            {
                return 0;
            }

            int playerDefense = targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.AllIncreased)[0]
                                - targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.AllDecreased)[0];

            byte playerDefenseUpgrade =
                (byte) (targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.DefenceLevelIncreased)[0]
                        - targetMate.GetBuff(CardType.Defence,
                            (byte) AdditionalTypes.Defence.DefenceLevelDecreased)[0]);

            int playerDodge = targetMate.GetBuff(CardType.DodgeAndDefencePercent,
                                  (byte) AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased)[0]
                              - targetMate.GetBuff(CardType.DodgeAndDefencePercent,
                                  (byte) AdditionalTypes.DodgeAndDefencePercent.DodgeDecreased)[0];

            int playerMorale = targetMate.Level +
                               targetMate.GetBuff(CardType.Morale, (byte) AdditionalTypes.Morale.MoraleIncreased)[0]
                               - targetMate.GetBuff(CardType.Morale, (byte) AdditionalTypes.Morale.MoraleDecreased)[0];

            int morale = Monster.Level + GetBuff(CardType.Morale, (byte) AdditionalTypes.Morale.MoraleIncreased)[0]
                         - GetBuff(CardType.Morale, (byte) AdditionalTypes.Morale.MoraleDecreased)[0];


            playerDefenseUpgrade += targetMate.Monster.DefenceUpgrade;

            short mainUpgrade = Monster.AttackUpgrade;
            int mainCritChance = Monster.CriticalChance;
            int mainCritHit = Monster.CriticalRate - 30;
            int mainMinDmg = Monster.DamageMinimum;
            int mainMaxDmg = Monster.DamageMaximum;
            int mainHitRate = Monster.Concentrate; //probably missnamed, check later
            if (mainMaxDmg == 0)
            {
                mainMinDmg = Monster.Level * 8;
                mainMaxDmg = Monster.Level * 12;
                mainCritChance = 10;
                mainCritHit = 120;
                mainHitRate = Monster.Level / 2 + 1;
            }

            #endregion

            #region Get Player defense

            skill?.BCards?.ToList().ForEach(s => SkillBcards.Add(s));

            int playerBoostpercentage;

            int boost = GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.AllAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.AllAttacksDecreased)[0];

            int boostpercentage = GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.DamageIncreased)[0]
                                  - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.DamageDecreased)[0];

            switch (Monster.AttackClass)
            {
                case 0:
                    playerDefense += targetMate.Defence;
                    playerDodge += targetMate.Monster.DefenceDodge;
                    playerBoostpercentage =
                        targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MeleeIncreased)[0]
                        - targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MeleeDecreased)[0];
                    playerDefense = (int) (playerDefense * (1 + playerBoostpercentage / 100D));

                    boost += GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.MeleeAttacksIncreased)[0]
                             - GetBuff(CardType.AttackPower,
                                 (byte) AdditionalTypes.AttackPower.MeleeAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MeleeIncreased)[0]
                                       - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MeleeDecreased)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int) (mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int) (mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 1:
                    playerDefense += targetMate.Monster.DistanceDefence;
                    playerDodge += targetMate.Monster.DistanceDefenceDodge;
                    playerBoostpercentage =
                        targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.RangedIncreased)[0]
                        - targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.RangedDecreased)[0];
                    playerDefense = (int) (playerDefense * (1 + playerBoostpercentage / 100D));

                    boost += GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.RangedAttacksIncreased)[0]
                             - GetBuff(CardType.AttackPower,
                                 (byte) AdditionalTypes.AttackPower.RangedAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.RangedIncreased)[0]
                                       - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.RangedDecreased)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int) (mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int) (mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                case 2:
                    playerDefense += targetMate.Monster.MagicDefence;
                    playerBoostpercentage =
                        targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MagicalIncreased)[0]
                        - targetMate.GetBuff(CardType.Defence, (byte) AdditionalTypes.Defence.MeleeDecreased)[0];
                    playerDefense = (int) (playerDefense * (1 + playerBoostpercentage / 100D));

                    boost +=
                        GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.MagicalAttacksIncreased)[0]
                        - GetBuff(CardType.AttackPower, (byte) AdditionalTypes.AttackPower.MagicalAttacksDecreased)[0];
                    boostpercentage += GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MagicalIncreased)[0]
                                       - GetBuff(CardType.Damage, (byte) AdditionalTypes.Damage.MagicalDecreased)[0];
                    mainMinDmg += boost;
                    mainMaxDmg += boost;
                    mainMinDmg = (int) (mainMinDmg * (1 + boostpercentage / 100D));
                    mainMaxDmg = (int) (mainMaxDmg * (1 + boostpercentage / 100D));
                    break;

                default:
                    throw new Exception($"Monster.AttackClass {Monster.AttackClass} not implemented");
            }

            #endregion

            #region Basic Damage Data Calculation

            mainCritChance +=
                targetMate.GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.ReceivingIncreased)[0]
                + GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.InflictingIncreased)[0]
                - targetMate.GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.ReceivingDecreased)[0]
                - GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.InflictingReduced)[0];

            mainCritHit += GetBuff(CardType.Critical, (byte) AdditionalTypes.Critical.DamageIncreased)[0]
                           - GetBuff(CardType.Critical,
                               (byte) AdditionalTypes.Critical.DamageIncreasedInflictingReduced)[0];

            // Critical damage deacreased by x %
            mainCritHit = (int) ((mainCritHit / 100D) * (100 + targetMate.GetBuff(CardType.Critical,
                                                             (byte) AdditionalTypes.Critical
                                                                 .DamageFromCriticalIncreased)[0]
                                                         - targetMate.GetBuff(CardType.Critical,
                                                             (byte) AdditionalTypes.Critical
                                                                 .DamageFromCriticalDecreased)[0]));

            mainUpgrade -= playerDefenseUpgrade;

            // Useless
            /*if (mainUpgrade < -10)
            {
                mainUpgrade = -10;
            }
            else if (mainUpgrade > 10)
            {
                mainUpgrade = 10;
            }*/

            #endregion

            #region Detailed Calculation

            #region Dodge

            double multiplier = playerDodge / (double) mainHitRate;
            if (multiplier > 5)
            {
                multiplier = 5;
            }
            double chance = -0.25 * Math.Pow(multiplier, 3) - 0.57 * Math.Pow(multiplier, 2) + 25.3 * multiplier - 1.41;
            if (chance <= 1)
            {
                chance = 1;
            }
            if (Monster.AttackClass == 0 || Monster.AttackClass == 1)
            {
                if (ServerManager.Instance.RandomNumber() <= chance)
                {
                    hitmode = 1;
                    return 0;
                }
            }

            #endregion

            #region Base Damage

            int baseDamage = ServerManager.Instance.RandomNumber(mainMinDmg, mainMaxDmg + 1);
            baseDamage += morale - playerMorale;

            switch (mainUpgrade)
            {
                case -10:
                    playerDefense += playerDefense * 2;
                    break;

                case -9:
                    playerDefense += (int) (playerDefense * 1.2);
                    break;

                case -8:
                    playerDefense += (int) (playerDefense * 0.9);
                    break;

                case -7:
                    playerDefense += (int) (playerDefense * 0.65);
                    break;

                case -6:
                    playerDefense += (int) (playerDefense * 0.54);
                    break;

                case -5:
                    playerDefense += (int) (playerDefense * 0.43);
                    break;

                case -4:
                    playerDefense += (int) (playerDefense * 0.32);
                    break;

                case -3:
                    playerDefense += (int) (playerDefense * 0.22);
                    break;

                case -2:
                    playerDefense += (int) (playerDefense * 0.15);
                    break;

                case -1:
                    playerDefense += (int) (playerDefense * 0.1);
                    break;

                case 0:
                    break;

                case 1:
                    baseDamage += (int) (baseDamage * 0.1);
                    break;

                case 2:
                    baseDamage += (int) (baseDamage * 0.15);
                    break;

                case 3:
                    baseDamage += (int) (baseDamage * 0.22);
                    break;

                case 4:
                    baseDamage += (int) (baseDamage * 0.32);
                    break;

                case 5:
                    baseDamage += (int) (baseDamage * 0.43);
                    break;

                case 6:
                    baseDamage += (int) (baseDamage * 0.54);
                    break;

                case 7:
                    baseDamage += (int) (baseDamage * 0.65);
                    break;

                case 8:
                    baseDamage += (int) (baseDamage * 0.9);
                    break;

                case 9:
                    baseDamage += (int) (baseDamage * 1.2);
                    break;

                case 10:
                    baseDamage += baseDamage * 2;
                    break;

                // Useless
                /*default:
                    if (mainUpgrade > 10)
                    {
                        baseDamage += baseDamage * (mainUpgrade / 5);
                    }
                    break;*/
            }

            #endregion

            #region Elementary Damage

            int elementalDamage = GetBuff(CardType.Element, (byte) AdditionalTypes.Element.AllIncreased)[0] -
                                  GetBuff(CardType.Element, (byte) AdditionalTypes.Element.AllDecreased)[0];

            int bonusrez = targetMate.GetBuff(CardType.ElementResistance,
                               (byte) AdditionalTypes.ElementResistance.AllIncreased)[0]
                           - targetMate.GetBuff(CardType.ElementResistance,
                               (byte) AdditionalTypes.ElementResistance.AllDecreased)[0];

            #region Calculate Elemental Boost + Rate

            double elementalBoost = 0;
            int playerRessistance = 0;
            switch (Monster.Element)
            {
                case 0:
                    break;

                case 1:
                    bonusrez += targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.FireIncreased)[0]
                                - targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.FireDecreased)[0];

                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.FireIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.FireDecreased)[0];

                    playerRessistance = targetMate.Monster.FireResistance;
                    switch (targetMate.Monster.Element)
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
                    bonusrez += targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.WaterIncreased)[0]
                                - targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.WaterDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.WaterIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.WaterDecreased)[0];
                    playerRessistance = targetMate.Monster.WaterResistance;
                    switch (targetMate.Monster.Element)
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
                    bonusrez += targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.LightIncreased)[0]
                                - targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.LightDecreased)[0];
                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.LightIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.LightDecreased)[0];
                    playerRessistance = targetMate.Monster.LightResistance;
                    switch (targetMate.Monster.Element)
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
                    bonusrez += targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.DarkIncreased)[0]
                                - targetMate.GetBuff(CardType.ElementResistance,
                                    (byte) AdditionalTypes.ElementResistance.DarkDecreased)[0];
                    playerRessistance = targetMate.Monster.DarkResistance;
                    elementalDamage += GetBuff(CardType.Element, (byte) AdditionalTypes.Element.DarkIncreased)[0]
                                       - GetBuff(CardType.Element, (byte) AdditionalTypes.Element.DarkDecreased)[0];
                    switch (targetMate.Monster.Element)
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

            if (Monster.Element == 0)
            {
                if (elementalBoost == 0.5)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1)
                {
                    elementalBoost = 0.05;
                }
                else if (elementalBoost == 1.3)
                {
                    elementalBoost = 0;
                }
                else if (elementalBoost == 1.5)
                {
                    elementalBoost = 0.15;
                }
                else if (elementalBoost == 2)
                {
                    elementalBoost = 0.2;
                }
                else if (elementalBoost == 3)
                {
                    elementalBoost = 0.2;
                }
            }
            elementalDamage =
                (int) ((elementalDamage + (100 + baseDamage) * (Monster.ElementRate / 100D)) * elementalBoost);
            elementalDamage = elementalDamage / 100 * (100 - playerRessistance - bonusrez);
            if (elementalDamage < 0)
            {
                elementalDamage = 0;
            }

            #endregion

            #region Critical Damage

            if (ServerManager.Instance.RandomNumber() <= mainCritChance)
            {
                if (Monster.AttackClass == 2)
                {
                }
                else
                {
                    baseDamage += (int) (baseDamage * (mainCritHit / 100D));
                    hitmode = 3;
                }
            }

            SkillBcards.Clear();

            #endregion

            #region Total Damage

            int totalDamage = baseDamage + elementalDamage -
                              (targetMate.HasBuff(CardType.SpecialDefence,
                                  (byte) AdditionalTypes.SpecialDefence.AllDefenceNullified)
                                  ? 0
                                  : playerDefense);
            if (totalDamage < 5)
            {
                totalDamage = ServerManager.Instance.RandomNumber(1, 6);
            }

            #endregion

            #endregion

            #region Minimum damage

            if (Monster.Level < 45)
            {
                //no minimum damage
            }
            else if (Monster.Level < 55)
            {
                totalDamage += Monster.Level;
            }
            else if (Monster.Level < 60)
            {
                totalDamage += Monster.Level * 2;
            }
            else if (Monster.Level < 65)
            {
                totalDamage += Monster.Level * 3;
            }
            else if (Monster.Level < 70)
            {
                totalDamage += Monster.Level * 4;
            }
            else
            {
                totalDamage += Monster.Level * 5;
            }

            #endregion

            #region Block

            if (ServerManager.Instance.RandomNumber() <
                targetMate.GetBuff(CardType.Block, (byte) AdditionalTypes.Block.ChanceAllIncreased)[0])
            {
                totalDamage = (totalDamage / 100) *
                              (100 - targetMate.GetBuff(CardType.Block,
                                   (byte) AdditionalTypes.Block.ChanceAllIncreased)[1]);
            }

            #endregion

            return totalDamage;
        }

        /// <summary>
        /// Generate the mv 3 packet
        /// </summary>
        /// <returns>string mv 3 packet</returns>
        private string GenerateMv3()
        {
            return $"mv 3 {MapMonsterId} {MapX} {MapY} {Monster.Speed}";
        }

        public void KillMonster(FactionType faction = FactionType.Neutral)
        {
            if (IsFactionTargettable(faction) && MonsterVNum != 679 && MonsterVNum != 680)
            {
                IsAlive = false;
                CurrentHp = 0;
                CurrentMp = 0;
                Death = DateTime.Now;
                LastMove = DateTime.Now.AddMilliseconds(500);
                Buff.Clear();
                Target = null;
            }
            else if (IsFactionTargettable(faction) && (MonsterVNum == 679 || MonsterVNum == 680))
            {
                IsAlive = true;
                CurrentHp = 1;
            }
            else
            {
                CurrentHp = 1;
            }
        }

        /// <summary>
        /// Handle any kind of Monster interaction
        /// </summary>
        private void MonsterLife()
        {
            if (Monster == null)
            {
                return;
            }

            // handle hit queue
            while (HitQueue.TryDequeue(out HitRequest hitRequest))
            {
                if (IsAlive && hitRequest.Session.Character.Hp > 0)
                {
                    int hitmode = 0;
                    switch (hitRequest.CasterType)
                    {
                        case UserType.Player:
                            bool onyxWings = false;
                            int damage = hitRequest.Session.Character.GenerateDamage(this, hitRequest.Skill,
                                ref hitmode, ref onyxWings);

                            if (onyxWings && MapInstance != null)
                            {
                                short onyxX = (short) (hitRequest.Session.Character.PositionX + 2);
                                short onyxY = (short) (hitRequest.Session.Character.PositionY + 2);
                                int onyxId = MapInstance.GetNextMonsterId();
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
                                MapInstance.Broadcast(
                                    $"guri 31 1 {hitRequest.Session.Character.CharacterId} {onyxX} {onyxY}");
                                onyx.Initialize(MapInstance);
                                MapInstance.AddMonster(onyx);
                                MapInstance.Broadcast(onyx.GenerateIn());
                                CurrentHp -= damage / 2;
                                HitRequest request = hitRequest;
                                Observable.Timer(TimeSpan.FromMilliseconds(350)).Subscribe(o =>
                                {
                                    MapInstance.Broadcast(
                                        $"su 3 {onyxId} 3 {MapMonsterId} -1 0 -1 {request.Skill.Effect} -1 -1 1 92 {damage / 2} 0 0");
                                    MapInstance.RemoveMonster(onyx);
                                    MapInstance.Broadcast(onyx.GenerateOut());
                                });
                            }
                            switch (hitRequest.TargetHitType)
                            {
                                case TargetHitType.SingleTargetHit:
                                    MapInstance?.Broadcast(
                                        $"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} {hitRequest.Session.Character.PositionX} {hitRequest.Session.Character.PositionY} {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {damage} {hitmode} {hitRequest.Skill.SkillType - 1}");
                                    break;

                                case TargetHitType.SingleTargetHitCombo:
                                    MapInstance?.Broadcast(
                                        $"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.SkillCombo.Animation} {hitRequest.SkillCombo.Effect} {hitRequest.Session.Character.PositionX} {hitRequest.Session.Character.PositionY} {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {damage} {hitmode} {hitRequest.Skill.SkillType - 1}");
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
                                    if (hitRequest.ShowTargetHitAnimation)
                                    {
                                        MapInstance?.Broadcast(
                                            $"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} 0 0 {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} 0 0 {hitRequest.Skill.SkillType - 1}");
                                    }
                                    MapInstance?.Broadcast(
                                        $"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} {hitRequest.Session.Character.PositionX} {hitRequest.Session.Character.PositionY} {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {damage} {hitmode} {hitRequest.Skill.SkillType - 1}");
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
                                    MapInstance?.Broadcast(
                                        $"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} {hitRequest.Session.Character.PositionX} {hitRequest.Session.Character.PositionY} {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {damage} {hitmode} {hitRequest.Skill.SkillType - 1}");
                                    break;

                                case TargetHitType.ZoneHit:
                                    MapInstance?.Broadcast(
                                        $"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} {hitRequest.MapX} {hitRequest.MapY} {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {damage} 5 {hitRequest.Skill.SkillType - 1}");
                                    break;

                                case TargetHitType.SpecialZoneHit:
                                    MapInstance?.Broadcast(
                                        $"su 1 {hitRequest.Session.Character.CharacterId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} {hitRequest.Session.Character.PositionX} {hitRequest.Session.Character.PositionY} {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {damage} 0 {hitRequest.Skill.SkillType - 1}");
                                    break;
                            }
                            break;

                        case UserType.Npc:
                            Mate mate = hitRequest.Session?.Character?.Mates.FirstOrDefault(x =>
                                x.MateTransportId == hitRequest.CasterId);
                            int mateDmg = mate.GenerateDamage(this, hitRequest.Skill, ref hitmode);
                            CurrentHp -= mateDmg;
                            MapInstance?.Broadcast(
                                $"su 2 {mate.MateTransportId} 3 {MapMonsterId} {hitRequest.Skill.SkillVNum} {hitRequest.Skill.Cooldown} {hitRequest.Skill.AttackAnimation} {hitRequest.SkillEffect} 0 0 {(IsAlive ? 1 : 0)} {(int) ((float) CurrentHp / (float) Monster.MaxHP * 100)} {mateDmg} {hitmode} 0");
                            break;
                    }
                    if (hitmode != 1)
                    {
                        hitRequest.Session.Character.RemoveBuff(85);
                    }

                    // generate the kill bonus
                    hitRequest.Session.Character.GenerateKillBonus(this);
                }
                else
                {
                    // monster already has been killed, send cancel
                    hitRequest.Session.SendPacket($"cancel 2 {MapMonsterId}");
                }
                if (IsBoss)
                {
                    MapInstance?.Broadcast(GenerateBoss());
                }
                else
                {
                    hitRequest.Skill.BCards.ToList().ForEach(b => b.ApplyBCards(this, hitRequest.Session.Character));
                }
            }

            // Respawn
            if (!IsAlive)
            {
                if (ShouldRespawn != null && ShouldRespawn.Value)
                {
                    double timeDeath = (DateTime.Now - Death).TotalSeconds;
                    if (timeDeath >= Monster.RespawnTime / 10d)
                    {
                        Respawn();
                    }
                }
                else
                {
                    Life.Dispose();
                }
            }
            // target following
            if (Target == null)
            {
                Move();
                return;
            }
            if (MapInstance == null)
            {
                return;
            }
            GetNearestOponent();
            HostilityTarget();

            short posX = 0;
            short posY = 0;

            if (Target is Character targetCharacter)
            {
                // remove target in some situations
                if (targetCharacter == null || targetCharacter.Invisible || targetCharacter.InvisibleGm ||
                    targetCharacter.Hp <= 0 || CurrentHp <= 0)
                {
                    RemoveTarget();
                    return;
                }
                posX = targetCharacter.PositionX;
                posY = targetCharacter.PositionY;
            }
            else if (Target is Mate targetMate)
            {
                // remove target in some situations
                if (targetMate == null || targetMate.Hp <= 0 || CurrentHp <= 0 || !targetMate.IsAlive)
                {
                    RemoveTarget();
                    return;
                }
                posX = targetMate.PositionX;
                posY = targetMate.PositionY;
            }

            lock (Target)
            {
                NpcMonsterSkill npcMonsterSkill = null;
                if (ServerManager.Instance.RandomNumber(0, 10) > 8 && Skills != null)
                {
                    npcMonsterSkill = Skills
                        .Where(s => (DateTime.Now - s.LastSkillUse).TotalMilliseconds >= 100 * s.Skill.Cooldown)
                        .OrderBy(rnd => _random.Next()).FirstOrDefault();
                }

                if (npcMonsterSkill?.Skill.TargetType == 1 && npcMonsterSkill?.Skill.HitType == 0)
                {
                    TargetHit(Target, npcMonsterSkill);
                }

                // check if target is in range
                if (npcMonsterSkill != null && CurrentMp >= npcMonsterSkill.Skill.MpCost
                    && Map.GetDistance(new MapCell
                        {
                            X = MapX,
                            Y = MapY
                        },
                        new MapCell
                        {
                            X = posX,
                            Y = posY
                        }) < npcMonsterSkill.Skill.Range)
                {
                    TargetHit(Target, npcMonsterSkill);
                }
                else if (Map.GetDistance(new MapCell
                             {
                                 X = MapX,
                                 Y = MapY
                             },
                             new MapCell
                             {
                                 X = posX,
                                 Y = posY
                             }) <= Monster.BasicRange)
                {
                    TargetHit(Target, npcMonsterSkill);
                }
                else
                {
                    FollowTarget();
                }
            }
            //HostilityTarget();
        }


        /// <summary>
        /// Broadcast effects applied on the current MapInstance
        /// </summary>
        public void ShowEffect()
        {
            if (!((DateTime.Now - LastEffect).TotalSeconds >= 5))
            {
                return;
            }
            if (IsTarget)
            {
                MapInstance.Broadcast(GenerateEff(824));
            }
            if (IsBonus)
            {
                MapInstance.Broadcast(GenerateEff(826));
            }
            LastEffect = DateTime.Now;
        }

        /// <summary>
        /// Generate rboss packet
        /// </summary>
        /// <returns>string rboss 3 packet</returns>
        public string GenerateBoss()
        {
            return $"rboss 3 {MapMonsterId} {CurrentHp} {Monster.MaxHP}";
        }

        private void Move()
        {
            // Normal Move Mode
            if (Monster == null || !IsAlive || HasBuff(CardType.Move, (byte)AdditionalTypes.Move.MovementImpossible))
            {
                return;
            }

            if (IsMoving && Monster.Speed > 0)
            {
                double time = (DateTime.Now - LastMove).TotalMilliseconds;
                if (!Path.Any() && time > _movetime && Target == null)
                {
                    short mapX = FirstX, mapY = FirstY;
                    if (MapInstance.Map?.GetFreePosition(ref mapX, ref mapY, (byte)ServerManager.Instance.RandomNumber(0, 2), (byte)_random.Next(0, 2)) ?? false)
                    {
                        int distance = Map.GetDistance(new MapCell
                        {
                            X = mapX,
                            Y = mapY
                        }, new MapCell
                        {
                            X = MapX,
                            Y = MapY
                        });

                        double value = 1000d * distance / (2 * Monster.Speed);
                        Observable.Timer(TimeSpan.FromMilliseconds(value))
                            .Subscribe(
                                x =>
                                {
                                    MapX = mapX;
                                    MapY = mapY;
                                });

                        LastMove = DateTime.Now.AddMilliseconds(value);
                        MapInstance.Broadcast(new BroadcastPacket(null, GenerateMv3(), ReceiverType.All));
                    }
                }
            }
            HostilityTarget();
        }

        /// <summary>
        /// Start the Respawn
        /// </summary>
        private void Respawn()
        {
            if (Monster == null)
            {
                return;
            }
            DamageList = new Dictionary<long, long>();
            IsAlive = true;
            Target = null;
            CurrentHp = Monster.MaxHP;
            CurrentMp = Monster.MaxMP;
            MapX = FirstX;
            MapY = FirstY;
            Path = new List<Node>();
            MapInstance.Broadcast(GenerateIn());
            Monster.BCards.ForEach(s => s.ApplyBCards(this));
        }

        /// <summary>
        /// Hit the Target Character.
        /// </summary>
        /// <param name="targetSession">Target session to hit</param>
        /// <param name="npcMonsterSkill">Skill use</param>
        private void TargetHit(object target, NpcMonsterSkill npcMonsterSkill)
        {
            if (Monster == null || (!((DateTime.Now - LastSkill).TotalMilliseconds >= 1000 + Monster.BasicCooldown * 200) && npcMonsterSkill == null) ||
                HasBuff(CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack))
            {
                return;
            }
            int hitmode = 0;

            int damage = 0;

            long targetId = -1;
            SessionType targetType = SessionType.Character;
            if (target is Character character)
            {
                if (!character.HasGodMode)
                {
                    damage = npcMonsterSkill != null ? GenerateDamage(character, npcMonsterSkill.Skill, ref hitmode) : GenerateDamage(character, null, ref hitmode);
                }
                if (character.IsSitting)
                {
                    character.IsSitting = false;
                    MapInstance.Broadcast(character.GenerateRest());
                }
                targetId = character.CharacterId;
            }
            else if (target is Mate mate)
            {
                damage = npcMonsterSkill != null ? GenerateDamage(mate, npcMonsterSkill.Skill, ref hitmode) : GenerateDamage(mate, null, ref hitmode);
                if (mate.IsSitting)
                {
                    mate.IsSitting = false;
                    MapInstance.Broadcast(mate.GenerateRest());
                }
                targetType = SessionType.Mate;
                targetId = mate.MateTransportId;
            }

            if (npcMonsterSkill != null)
            {
                if (CurrentMp < npcMonsterSkill.Skill.MpCost)
                {
                    FollowTarget();
                    return;
                }
                npcMonsterSkill.LastSkillUse = DateTime.Now;
                CurrentMp -= npcMonsterSkill.Skill.MpCost;
                MapInstance.Broadcast($"ct 3 {MapMonsterId} {(byte) targetType} {targetId} {npcMonsterSkill.Skill.CastAnimation} {npcMonsterSkill.Skill.CastEffect} {npcMonsterSkill.Skill.SkillVNum}");
            }
            LastMove = DateTime.Now;

            int castTime = 0;
            if (npcMonsterSkill != null && npcMonsterSkill.Skill.CastEffect != 0)
            {
                MapInstance.Broadcast(GenerateEff(npcMonsterSkill.Skill.CastEffect), MapX, MapY);
                castTime = npcMonsterSkill.Skill.CastTime * 100;
            }
            Observable.Timer(TimeSpan.FromMilliseconds(castTime))
                .Subscribe(
                    o =>
                    {
                        TargetHit2(target, npcMonsterSkill, damage, hitmode);
                    });
        }

        /// <summary>
        /// Hit the Character
        /// </summary>
        /// <param name="targetSession">Target Session to hit</param>
        /// <param name="npcMonsterSkill">Skill to use</param>
        /// <param name="damage">Number of damage to hit</param>
        /// <param name="hitmode">hit mode to use</param>
        private void TargetHit2(object target, NpcMonsterSkill npcMonsterSkill, int damage, int hitmode)
        {
            short posX = 0;
            short posY = 0;
            int hp = 0;

            if (target is Character character)
            {
                if (character.Hp > 0)
                {
                    character.GetDamage(damage);

                    MapInstance.Broadcast(null,
                        ServerManager.Instance.GetUserMethod<string>(character.CharacterId, "GenerateStat"),
                        ReceiverType.OnlySomeone, "", character.CharacterId);
                    MapInstance.Broadcast(npcMonsterSkill != null
                        ? $"su 3 {MapMonsterId} 1 {character.CharacterId} {npcMonsterSkill.SkillVNum} {npcMonsterSkill.Skill.Cooldown} {npcMonsterSkill.Skill.AttackAnimation} {npcMonsterSkill.Skill.Effect} {MapX} {MapY} {(character.Hp > 0 ? 1 : 0)} {(int)(character.Hp / character.HpLoad() * 100)} {damage} {hitmode} 0"
                        : $"su 3 {MapMonsterId} 1 {character.CharacterId} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(character.Hp > 0 ? 1 : 0)} {(int)(character.Hp / character.HpLoad() * 100)} {damage} {hitmode} 0");
                    LastSkill = DateTime.Now;

                    // SP3M Frozen Shield
                    if (character.HasBuff(CardType.SecondSPCard, (byte)AdditionalTypes.SecondSPCard.HitAttacker))
                    {
                        if (ServerManager.Instance.RandomNumber() < character.GetBuff(CardType.SecondSPCard,
                                (byte)AdditionalTypes.SecondSPCard.HitAttacker)[0])
                        {
                            AddBuff(new Buff(character.GetBuff(CardType.SecondSPCard,(byte) AdditionalTypes.SecondSPCard.HitAttacker)[1], character.Level));
                        }
                    }

                    npcMonsterSkill?.Skill.BCards.ToList().ForEach(s =>
                    {
                        Buff b = new Buff(s.SecondData);

                        switch (b.Card?.BuffType)
                        {
                            case BuffType.Bad:
                                s.ApplyBCards(character);
                                break;

                            case BuffType.Good:
                            case BuffType.Neutral:
                                s.ApplyBCards(this);
                                break;
                        }
                    });
                    if (character.Hp <= 0)
                    {
                        RemoveTarget();
                        Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(o => { ServerManager.Instance.AskRevive(character.CharacterId); });
                    }
                }
            }
            else if (target is Mate mate)
            {
                if (mate.IsAlive)
                {
                    mate.GetDamage(damage);

                    MapInstance.Broadcast(npcMonsterSkill != null
                        ? $"su 3 {MapMonsterId} 2 {mate.MateTransportId} {npcMonsterSkill.SkillVNum} {npcMonsterSkill.Skill.Cooldown} {npcMonsterSkill.Skill.AttackAnimation} {npcMonsterSkill.Skill.Effect} {MapX} {MapY} {(mate.Hp > 0 ? 1 : 0)} {(int)(mate.Hp / mate.HpLoad() * 100)} {damage} {hitmode} 0"
                        : $"su 3 {MapMonsterId} 2 {mate.MateTransportId} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(mate.Hp > 0 ? 1 : 0)} {(int)(mate.Hp / mate.HpLoad() * 100)} {damage} {hitmode} 0");
                    npcMonsterSkill?.Skill.BCards.ToList().ForEach(s => s.ApplyBCards(mate));
                    LastSkill = DateTime.Now;

                    if (mate.Hp <= 0)
                    {
                        RemoveTarget();
                        mate.GenerateDeath();
                    }
                }
            }


            if (npcMonsterSkill == null || (npcMonsterSkill.Skill.Range <= 0 && npcMonsterSkill.Skill.TargetRange <= 0))
            {
                return;
            }

            foreach (Character characterInRange in MapInstance
                .GetCharactersInRange(npcMonsterSkill.Skill.TargetRange == 0 ? MapX : posX,
                    npcMonsterSkill.Skill.TargetRange == 0 ? MapY : posY, npcMonsterSkill.Skill.TargetRange)
                .Where(s => s != Target && s.Hp > 0 && !s.InvisibleGm))
            {
                if (characterInRange.IsSitting)
                {
                    characterInRange.IsSitting = false;
                    MapInstance.Broadcast(characterInRange.GenerateRest());
                }
                if (characterInRange.HasGodMode)
                {
                    damage = 0;
                    hitmode = 1;
                }
                if (characterInRange.Hp <= 0)
                {
                    continue;
                }
                characterInRange.GetDamage(damage);
                MapInstance.Broadcast(null, characterInRange.GenerateStat(), ReceiverType.OnlySomeone, "", characterInRange.CharacterId);
                MapInstance.Broadcast(
                    $"su 3 {MapMonsterId} 1 {characterInRange.CharacterId} 0 {Monster.BasicCooldown} 11 {Monster.BasicSkill} 0 0 {(characterInRange.Hp > 0 ? 1 : 0)} {(int)(characterInRange.Hp / characterInRange.HpLoad() * 100)} {damage} {hitmode} 0");
                if (characterInRange.Hp > 0)
                {
                    continue;
                }
                RemoveTarget();
                Observable.Timer(TimeSpan.FromMilliseconds(1000)).Subscribe(o => { ServerManager.Instance.AskRevive(characterInRange.CharacterId); });
            }

            foreach (Mate mateInRange in MapInstance.GetMatesInRange(npcMonsterSkill.Skill.TargetRange == 0 ? MapX : posX,
                    npcMonsterSkill.Skill.TargetRange == 0 ? MapY : posY, npcMonsterSkill.Skill.TargetRange)
                .Where(s => s != Target && s.IsAlive))
            {
                if (mateInRange.IsSitting)
                {
                    mateInRange.IsSitting = false;
                    MapInstance.Broadcast(mateInRange.GenerateRest());
                }
                mateInRange.GetDamage(damage);

                MapInstance.Broadcast($"su 3 {MapMonsterId} 2 {mateInRange.MateTransportId} {npcMonsterSkill.SkillVNum} {npcMonsterSkill.Skill.Cooldown} {npcMonsterSkill.Skill.AttackAnimation} {npcMonsterSkill.Skill.Effect} {MapX} {MapY} {(mateInRange.Hp > 0 ? 1 : 0)} {(int)(mateInRange.Hp / mateInRange.HpLoad() * 100)} {damage} {hitmode} 0");
                npcMonsterSkill.Skill.BCards.ToList().ForEach(s => s.ApplyBCards(mateInRange));
                LastSkill = DateTime.Now;

                if (mateInRange.Hp <= 0)
                {
                    RemoveTarget();
                    mateInRange.GenerateDeath();
                }
            }
        }

        /// <summary>
        /// Add the buff
        /// </summary>
        /// <param name="indicator">Buff to add</param>
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

        /// <summary>
        /// Remove buff from Buff Container
        /// </summary>
        /// <param name="id">Card Id to remove</param>
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

        /// <summary>
        /// Get Buffs
        /// </summary>
        /// <param name="type">CardType</param>
        /// <param name="subtype"></param>
        /// <param name="affectingOpposite"></param>
        /// <returns>Param1 = FirstData | Param2 = SecondData</returns>
        public int[] GetBuff(CardType type, byte subtype, bool affectingOpposite = false)
        {
            int value1 = 0;
            int value2 = 0;

            foreach (BCard entry in SkillBcards.Where(s => s != null && s.Type.Equals((byte)type) && s.SubType.Equals(subtype)))
            {
                if (entry.IsLevelScaled)
                {
                    if (entry.IsLevelDivided)
                    {
                        value1 += Monster.Level / entry.FirstData;
                    }
                    else
                    {
                        value1 += entry.FirstData * Monster.Level;
                    }
                }
                else
                {
                    value1 += entry.FirstData;
                }
                value2 += entry.SecondData;
            }

            foreach (Buff buff in Buff)
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

        /// <summary>
        /// Check if the Entity has the MapMonster
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subtype"></param>
        /// <returns>true if has buff</returns>
        public bool HasBuff(CardType type, byte subtype)
        {
            return Buff.Any(buff =>
                buff.Card.BCards.Any(b => b.Type == (byte)type && b.SubType == subtype && (b.CastType != 1 || b.CastType == 1 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now)));
        }

        #endregion
    }
}