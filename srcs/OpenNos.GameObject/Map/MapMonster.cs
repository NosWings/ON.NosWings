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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Data;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.GameObject.Packets.ServerPackets;
using OpenNos.PathFinder.PathFinder;
using OpenNos.GameObject.Battle;

namespace OpenNos.GameObject.Map
{
    public class MapMonster : MapMonsterDTO, IBattleEntity
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

        public ConcurrentBag<Buff.Buff> Buff { get; internal set; }

        public ConcurrentBag<BCard> SkillBcards { get; set; }

        public int CurrentHp { get; set; }

        public int CurrentMp { get; set; }

        public IDictionary<IBattleEntity, long> DamageList { get; private set; }

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

        public IBattleEntity Target { get; set; }

        public short FirstX { get; set; }

        public short FirstY { get; set; }

        public bool IsPercentage { get; set; }

        public int TakesDamage { get; set; }

        public int GiveDamagePercent { get; set; }

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
                    $"in 3 {MonsterVNum} {MapMonsterId} {MapX} {MapY} {Position} {(int)((float)CurrentHp / (float)Monster.MaxHP * 100)} {(int)((float)CurrentMp / (float)Monster.MaxMP * 100)} 0 0 0 -1 {(Monster.NoAggresiveIcon ? (byte)InRespawnType.NoEffect : (byte)InRespawnType.TeleportationEffect)} 0 -1 - 0 -1 0 0 0 0 0 0 0 0";
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
            DamageList = new Dictionary<IBattleEntity, long>();
            _random = new Random(MapMonsterId);
            _movetime = ServerManager.Instance.RandomNumber(400, 3200);
            Buff = new ConcurrentBag<Buff.Buff>();
            SkillBcards = new ConcurrentBag<BCard>();
            IsPercentage = Monster.IsPercent;
            TakesDamage = Monster.TakeDamages;
            GiveDamagePercent = Monster.GiveDamagePercentage;
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
            if (MapInstance?.IsSleeping == false && Life == null)
            {
                Life = Observable.Interval(TimeSpan.FromMilliseconds(400)).Subscribe(x =>
                {
                    try
                    {
                        if (MapInstance?.IsSleeping == false)
                        {
                            MonsterLife();
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                    }
                });
            }
        }

        public void StopLife()
        {
            Life?.Dispose();
            Life = null;
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
            Target = DamageList.Keys.ToList().OrderBy(e => Map.GetDistance(GetPos(), e.GetPos())).FirstOrDefault(e => e.isTargetable(GetSessionType()));
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
            IBattleEntity target = MapInstance.BattleEntities.FirstOrDefault(e => e.isTargetable(GetSessionType()) && Map.GetDistance(GetPos(), e.GetPos()) < (NoticeRange == 0 ? Monster.NoticeRange : NoticeRange));

            if (target == null)
            {
                return;
            }

            if (!OnNoticeEvents.Any() && MoveEvent == null)
            {
                Target = target;
                if (!Monster.NoAggresiveIcon && LastEffect.AddSeconds(5) < DateTime.Now && target.GetSession() is Character character)
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
            Path.Clear();
            Target = null;
            //return to origin
            Path = BestFirstSearch.FindPath(new Node { X = MapX, Y = MapY }, new Node { X = FirstX, Y = FirstY }, MapInstance.Map.Grid);
        }

        /// <summary>
        /// Follow the Monsters target to it's position.
        /// </summary>
        /// <param name="targetSession">The TargetSession to follow</param>
        private void FollowTarget()
        {
            if (Monster == null || !IsAlive || HasBuff(BCardType.CardType.Move, (byte)AdditionalTypes.Move.MovementImpossible) ||
                !IsMoving)
            {
                return;
            }

            if (Target == null)
            {
                RemoveTarget();
                return;
            }

            if (!Path.Any())
            {
                try
                {
                    List<Node> list = BestFirstSearch.TracePath(new Node() { X = MapX, Y = MapY }, Target.GetBrushFire(), MapInstance.Map.Grid);
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
            int distance = Map.GetDistance(Target.GetPos(), new MapCell { X = MapX, Y = MapY });
            if (Monster != null && DateTime.Now > LastMove && Monster.Speed > 0 && Path.Any())
            {
                int maxindex = Path.Count > Monster.Speed / 2 ? Monster.Speed / 2 : Path.Count;
                short smapX = Path[maxindex - 1].X;
                short smapY = Path[maxindex - 1].Y;
                double waitingtime =
                    Map.GetDistance(new MapCell { X = smapX, Y = smapY }, new MapCell { X = MapX, Y = MapY }) /
                    (double)Monster.Speed;
                MapInstance.Broadcast(new BroadcastPacket(null, $"mv 3 {MapMonsterId} {smapX} {smapY} {Monster.Speed}",
                    ReceiverType.All, xCoordinate: smapX, yCoordinate: smapY));
                LastMove = DateTime.Now.AddSeconds(waitingtime > 1 ? 1 : waitingtime);

                Observable.Timer(TimeSpan.FromMilliseconds((int)((waitingtime > 1 ? 1 : waitingtime) * 1000)))
                    .Subscribe(x =>
                    {
                        MapX = smapX;
                        MapY = smapY;
                    });
                distance = (int)Path[0].F;
                Path.RemoveRange(0, maxindex);
                if (distance > (maxDistance) + 3)
                {
                    RemoveTarget();
                }
            }
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

            if (!IsAlive) // Respawn
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
            
            if (Target == null) // target following
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
                    TargetHit(npcMonsterSkill);
                }

                // check if target is in range
                if (npcMonsterSkill != null && CurrentMp >= npcMonsterSkill.Skill.MpCost && Map.GetDistance(GetPos(), Target.GetPos()) < npcMonsterSkill.Skill.Range)
                {
                    TargetHit(npcMonsterSkill);
                }
                else if (Map.GetDistance(GetPos(), Target.GetPos()) <= Monster.BasicRange)
                {
                    TargetHit(npcMonsterSkill);
                }
                else
                {
                    FollowTarget();
                }
            }
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
            if (Monster == null || !IsAlive || HasBuff(BCardType.CardType.Move, (byte)AdditionalTypes.Move.MovementImpossible))
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
                        int distance = Map.GetDistance(new MapCell { X = mapX, Y = mapY }, GetPos());

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
            DamageList = new Dictionary<IBattleEntity, long>();
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

        public void TargetHit(NpcMonsterSkill npcMonsterSkill)
        {
            if (Monster == null || (!((DateTime.Now - LastSkill).TotalMilliseconds >= 1000 + Monster.BasicCooldown * 250) && npcMonsterSkill == null) || HasBuff(BCardType.CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack))
            {
                return;
            }
            Skill skill = npcMonsterSkill?.Skill ?? ServerManager.Instance.GetSkill(Monster.BasicSkill);

            if (skill == null || CurrentMp < skill.MpCost)
            {
                FollowTarget();
                return;
            }
            if (npcMonsterSkill != null)
            {
                npcMonsterSkill.LastSkillUse = DateTime.Now;
                CurrentMp -= npcMonsterSkill.Skill.MpCost;
            }
            MapInstance.Broadcast($"ct 3 {MapMonsterId} {(byte)Target.GetSessionType()} {Target.GetId()} {skill.CastAnimation} {skill.CastEffect} {skill.SkillVNum}");
            LastMove = DateTime.Now;
            GetInformations().TargetHit(Target, TargetHitType.SingleTargetHit, skill);
        }

        /// <summary>
        /// Add the buff
        /// </summary>
        /// <param name="indicator">Buff to add</param>
        public void AddBuff(Buff.Buff indicator)
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
            Buff.Buff indicator = Buff.FirstOrDefault(s => s.Card.CardId == id);
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
        public int[] GetBuff(BCardType.CardType type, byte subtype, bool affectingOpposite = false)
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

            foreach (Buff.Buff buff in Buff)
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
        public bool HasBuff(BCardType.CardType type, byte subtype)
        {
            return Buff.Any(buff =>
                buff.Card.BCards.Any(b => b.Type == (byte)type && b.SubType == subtype && (b.CastType != 1 || b.CastType == 1 && buff.Start.AddMilliseconds(buff.Card.Delay * 100) < DateTime.Now)));
        }

        public MapCell GetPos()
        {
            return new MapCell { X = MapX, Y = MapY };
        }

        public BattleEntity GetInformations()
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
            return type != SessionType.Monster && IsAlive && CurrentHp > 0;
        }

        public Node[,] GetBrushFire()
        {
            return BestFirstSearch.LoadBrushFire(new GridPos() { X = MapX, Y = MapY }, MapInstance.Map.Grid);
        }

        public SessionType GetSessionType()
        {
            return SessionType.Monster;
        }

        public long GetId()
        {
            return MapMonsterId;
        }

        public MapInstance GetMapInstance()
        {
            return MapInstance;
        }

        public int[] GetHp()
        {
            return new[] { CurrentHp, Monster.MaxHP };
        }

        public void GetDamage(int damage, bool canKill = true)
        {
            CurrentHp -= damage;
            if (CurrentHp <= 0)
            {
                CurrentHp = 0;
            }
            if (!canKill && CurrentHp == 0)
            {
                CurrentHp = 1;
            }
        }

        public void GenerateDeath(IBattleEntity killer = null)
        {
            killer?.GenerateRewards(this);
            if (MonsterVNum != 679 && MonsterVNum != 680) // Act4 Guardians
            {
                IsAlive = false;
                CurrentHp = 0;
                CurrentMp = 0;
                Death = DateTime.Now;
                LastMove = DateTime.Now.AddMilliseconds(500);
                Buff.Clear();
                Target = null;
                return;
            }
            CurrentHp = 1;
        }

        public void GenerateRewards(IBattleEntity target)
        {
            RemoveTarget();
        }

        #endregion
    }
}