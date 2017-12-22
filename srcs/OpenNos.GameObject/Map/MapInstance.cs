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
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.PathFinder.PathFinder;
using OpenNos.GameObject.Battle;

namespace OpenNos.GameObject.Map
{
    public class MapInstance : BroadcastableBase
    {
        #region Instantiation

        public MapInstance(Map map, Guid guid, bool shopAllowed, MapInstanceType type, InstanceBag instanceBag)
        {
            Buttons = new List<MapButton>();
            XpRate = 1;
            DropRate = 1;
            ShopAllowed = shopAllowed;
            MapInstanceType = type;
            _isSleeping = true;
            LastUserShopId = 0;
            InstanceBag = instanceBag;
            Clock = new Clock(3);
            _random = new Random();
            Map = map;
            MapInstanceId = guid;
            UnlockEvents = new ConcurrentBag<EventContainer>();
            ScriptedInstances = new List<ScriptedInstance>();
            OnCharacterDiscoveringMapEvents = new ConcurrentBag<Tuple<EventContainer, List<long>>>();
            OnMoveOnMapEvents = new ConcurrentBag<EventContainer>();
            OnAreaEntryEvents = new ConcurrentBag<ZoneEvent>();
            WaveEvents = new ConcurrentBag<EventWave>();
            OnMapClean = new ConcurrentBag<EventContainer>();
            _monsters = new ConcurrentDictionary<long, MapMonster>();
            _npcs = new ConcurrentDictionary<long, MapNpc>();
            _mapMonsterIds = new ConcurrentBag<int>();
            _mapNpcIds = new ConcurrentBag<int>();
            DroppedList = new ConcurrentDictionary<long, MapItem>();
            Portals = new List<Portal>();
            UserShops = new Dictionary<long, MapShop>();
            MapDesignObjects = new ConcurrentBag<MapDesignObject>();
            MonsterLocker = new Locker();
            ButtonLocker = new Locker();
            StartLife();
        }

        #endregion

        #region Members

        private readonly ConcurrentBag<int> _mapMonsterIds;

        private readonly ConcurrentBag<int> _mapNpcIds;

        private readonly ConcurrentDictionary<long, MapMonster> _monsters;

        private readonly ConcurrentDictionary<long, MapNpc> _npcs;

        private readonly Random _random;

        private bool _disposed;

        private bool _isSleeping;

        private bool _isSleepingRequest;

        #endregion

        #region Properties

        public Locker ButtonLocker { get; set; }

        public List<MapButton> Buttons { get; set; }

        public Clock Clock { get; set; }

        public ConcurrentDictionary<long, MapItem> DroppedList { get; }

        public int DropRate { get; set; }

        public ConcurrentBag<MapDesignObject> MapDesignObjects { get; set; }

        public Locker MonsterLocker { get; set; }

        public InstanceBag InstanceBag { get; set; }

        public bool IsMute { get; set; }

        public bool IsDancing { get; set; }

        public bool IsPvp { get; set; }

        // TODO NEED A REVIEW
        public bool IsSleeping
        {
            get
            {
                if (!_isSleepingRequest || _isSleeping || LastUnregister.AddSeconds(20) >= DateTime.Now || Sessions.Any())
                {
                    return _isSleeping;
                }
                _isSleeping = true;
                _isSleepingRequest = false;
                Parallel.ForEach(Monsters.Where(s => s.Life != null), m => { m.StopLife(); });
                Parallel.ForEach(Npcs.Where(s => s.Life != null), m => { m.StopLife(); });
                return true;
            }
            set
            {
                if (value)
                {
                    _isSleepingRequest = true;
                }
                else
                {
                    _isSleeping = false;
                    _isSleepingRequest = false;
                    Parallel.ForEach(Monsters.Where(s => s.Life == null), m => { m.StartLife(); });
                    Parallel.ForEach(Npcs.Where(s => s.Life == null), m => { m.StartLife(); });
                }
            }
        }

        public long LastUserShopId { get; set; }

        public Map Map { get; set; }

        public byte MapIndexX { get; set; }

        public byte MapIndexY { get; set; }

        public Guid MapInstanceId { get; set; }

        public MapInstanceType MapInstanceType { get; set; }

        public List<MapMonster> Monsters
        {
            get { return _monsters.Select(s => s.Value).ToList(); }
        }

        public List<MapNpc> Npcs
        {
            get { return _npcs.Select(s => s.Value).ToList(); }
        }

        public ConcurrentBag<Tuple<EventContainer, List<long>>> OnCharacterDiscoveringMapEvents { get; }

        public ConcurrentBag<EventContainer> OnMapClean { get; }

        public ConcurrentBag<EventContainer> OnMoveOnMapEvents { get; }

        public ConcurrentBag<ZoneEvent> OnAreaEntryEvents { get; }

        public ConcurrentBag<EventWave> WaveEvents { get; }

        public List<Portal> Portals { get; }

        public bool ShopAllowed { get; }

        public List<ScriptedInstance> ScriptedInstances { get; }

        public ConcurrentBag<EventContainer> UnlockEvents { get; set; }

        public Dictionary<long, MapShop> UserShops { get; }

        public int XpRate { get; set; }

        private IDisposable Life { get; set; }

        #endregion

        #region Methods

        public IEnumerable<IBattleEntity> BattleEntities
        {
            get
            {
                return _battleEntities.Select(e => e.Value).Concat(Npcs).Concat(Monsters);
            }
        }

        public void AddMonster(MapMonster monster)
        {
            _monsters[monster.MapMonsterId] = monster;
        }

        public void AddNpc(MapNpc monster)
        {
            _npcs[monster.MapNpcId] = monster;
        }

        public sealed override void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            Dispose(true);
            GC.SuppressFinalize(this);
            _disposed = true;
        }

        public void DropItemByMonster(long? owner, DropDTO drop, short mapX, short mapY, bool isQuest = false)
        {
            // TODO: Parallelize, if possible.
            try
            {
                short localMapX = mapX;
                short localMapY = mapY;
                List<MapCell> possibilities = new List<MapCell>();

                for (short x = -1; x < 2; x++)
                {
                    for (short y = -1; y < 2; y++)
                    {
                        possibilities.Add(new MapCell { X = x, Y = y });
                    }
                }

                foreach (MapCell possibilitie in possibilities.OrderBy(s => ServerManager.Instance.RandomNumber()))
                {
                    localMapX = (short)(mapX + possibilitie.X);
                    localMapY = (short)(mapY + possibilitie.Y);
                    if (!Map.IsBlockedZone(localMapX, localMapY))
                    {
                        break;
                    }
                }

                MonsterMapItem droppedItem = new MonsterMapItem(localMapX, localMapY, drop.ItemVNum, drop.Amount, owner ?? -1);
                DroppedList[droppedItem.TransportId] = droppedItem;
                Broadcast(
                    $"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)} {(isQuest ? 1 : 0)} 0 -1");
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void DropItems(List<Tuple<short, int, short, short>> list)
        {
            // TODO: Parallelize, if possible.
            foreach (Tuple<short, int, short, short> drop in list)
            {
                MonsterMapItem droppedItem = new MonsterMapItem(drop.Item3, drop.Item4, drop.Item1, drop.Item2);
                DroppedList[droppedItem.TransportId] = droppedItem;
                Broadcast(
                    $"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)} 0 0 -1");
            }
        }

        private IEnumerable<string> GenerateNpcShopOnMap()
        {
            return (from npc in Npcs where npc.Shop != null select $"shop 2 {npc.MapNpcId} {npc.Shop.ShopId} {npc.Shop.MenuType} {npc.Shop.ShopType} {npc.Shop.Name}").ToList();
        }

        private IEnumerable<string> GeneratePlayerShopOnMap()
        {
            return UserShops.Select(shop => $"pflag 1 {shop.Value.OwnerId} {shop.Key + 1}").ToList();
        }

        public string GenerateRsfn(bool isInit = false)
        {
            return MapInstanceType == MapInstanceType.TimeSpaceInstance ? $"rsfn {MapIndexX} {MapIndexY} {(isInit ? 1 : (Monsters.Where(s => s.IsAlive).ToList().Count == 0 ? 0 : 1))}" : string.Empty;
        }

        private IEnumerable<string> GenerateUserShops()
        {
            return UserShops.Select(shop => $"shop 1 {shop.Value.OwnerId} 1 3 0 {shop.Value.Name}").ToList();
        }

        public List<MapMonster> GetListMonsterInRange(short mapX, short mapY, byte distance)
        {
            return _monsters.Select(s => s.Value).Where(s => s.IsAlive && s.IsInRange(mapX, mapY, distance)).ToList();
        }

        public List<string> GetMapItems()
        {
            List<string> packets = new List<string>();
            Portals.ForEach(s => packets.Add(s.GenerateGp()));
            ScriptedInstances.Where(s => s.Type == ScriptedInstanceType.TimeSpace).ToList().ForEach(s => packets.Add(s.GenerateWp()));
            Monsters.ForEach(s =>
            {
                packets.Add(s.GenerateIn());
                if (s.IsBoss)
                {
                    packets.Add(s.GenerateBoss());
                }
            });
            Npcs.ForEach(s => { packets.Add(s.GenerateIn()); });
            packets.AddRange(GenerateNpcShopOnMap());
            Parallel.ForEach(DroppedList.Select(s => s.Value), session => { packets.Add(session.GenerateIn()); });
            Buttons.ForEach(s => { packets.Add(s.GenerateIn()); });
            packets.AddRange(GenerateUserShops());
            packets.AddRange(GeneratePlayerShopOnMap());
            return packets;
        }

        public MapMonster GetMonster(long mapMonsterId)
        {
            return !_monsters.ContainsKey(mapMonsterId) ? null : _monsters[mapMonsterId];
        }

        // TODO: Fix, Seems glitchy.
        public int GetNextMonsterId()
        {
            int nextId = _mapMonsterIds.Any() ? _mapMonsterIds.LastOrDefault() + 1 : 1;
            _mapMonsterIds.Add(nextId);
            return nextId;
        }

        // TODO: Fix, Seems glitchy.
        private int GetNextNpcId()
        {
            int nextId = _mapNpcIds.Any() ? _mapNpcIds.LastOrDefault() + 1 : 1;
            _mapNpcIds.Add(nextId);
            return nextId;
        }

        public void LoadMonsters()
        {
            OrderablePartitioner<MapMonsterDTO> partitioner = Partitioner.Create(DaoFactory.MapMonsterDao.LoadFromMap(Map.MapId), EnumerablePartitionerOptions.None);
            Parallel.ForEach(partitioner, monster =>
            {
                if (!(monster is MapMonster mapMonster))
                {
                    return;
                }
                mapMonster.Initialize(this);
                int mapMonsterId = mapMonster.MapMonsterId;
                _monsters[mapMonsterId] = mapMonster;
                _mapMonsterIds.Add(mapMonsterId);
            });
        }

        public void LoadNpcs()
        {
            OrderablePartitioner<MapNpcDTO> partitioner = Partitioner.Create(DaoFactory.MapNpcDao.LoadFromMap(Map.MapId), EnumerablePartitionerOptions.None);
            Parallel.ForEach(partitioner, npc =>
            {
                if (!(npc is MapNpc mapNpc))
                {
                    return;
                }
                mapNpc.Initialize(this);
                int mapNpcId = mapNpc.MapNpcId;
                _npcs[mapNpcId] = mapNpc;
                _mapNpcIds.Add(mapNpcId);
            });
        }

        public void LoadPortals()
        {
            OrderablePartitioner<PortalDTO> partitioner = Partitioner.Create(DaoFactory.PortalDao.LoadByMap(Map.MapId), EnumerablePartitionerOptions.None);
            ConcurrentDictionary<int, Portal> portalList = new ConcurrentDictionary<int, Portal>();
            Parallel.ForEach(partitioner, portal =>
            {
                if (!(portal is Portal portal2))
                {
                    return;
                }
                portal2.SourceMapInstanceId = MapInstanceId;
                portalList[portal2.PortalId] = portal2;
            });
            Portals.AddRange(portalList.Select(s => s.Value));
        }

        public void MapClear()
        {
            Broadcast("mapclear");
            GetMapItems().ForEach(Broadcast);
        }

        public string GenerateMapDesignObjects()
        {
            string mlobjstring = "mltobj";
            int i = 0;
            foreach (MapDesignObject mp in MapDesignObjects)
            {
                mlobjstring += $" {mp.ItemInstance.ItemVNum}.{i}.{mp.MapX}.{mp.MapY}";
                i++;
            }
            return mlobjstring;
        }

        public IEnumerable<string> GetMapDesignObjectEffects()
        {
            return MapDesignObjects.Select(mp => mp.GenerateEffect(false)).ToList();
        }

        public MapItem PutItem(InventoryType type, short slot, byte amount, ref ItemInstance inv, ClientSession session)
        {
            Guid random2 = Guid.NewGuid();
            List<GridPos> possibilities = new List<GridPos>();

            for (short x = -2; x < 3; x++)
            {
                for (short y = -2; y < 3; y++)
                {
                    possibilities.Add(new GridPos { X = x, Y = y });
                }
            }

            short mapX = 0;
            short mapY = 0;
            bool niceSpot = false;
            foreach (GridPos possibility in possibilities.OrderBy(s => _random.Next()))
            {
                mapX = (short)(session.Character.PositionX + possibility.X);
                mapY = (short)(session.Character.PositionY + possibility.Y);
                if (Map.IsBlockedZone(mapX, mapY))
                {
                    continue;
                }
                niceSpot = true;
                break;
            }

            if (!niceSpot)
            {
                return null;
            }
            if (amount <= 0 || amount > inv.Amount)
            {
                return null;
            }
            ItemInstance newItemInstance = inv.DeepCopy();
            newItemInstance.Id = random2;
            newItemInstance.Amount = amount;
            MapItem droppedItem = new CharacterMapItem(mapX, mapY, newItemInstance);
            DroppedList[droppedItem.TransportId] = droppedItem;
            inv.Amount -= amount;
            return droppedItem;
        }

        private void RemoveMapItem()
        {
            try
            {
                List<MapItem> dropsToRemove = DroppedList.Select(s => s.Value).Where(dl => dl.CreatedDate.AddMinutes(3) < DateTime.Now).ToList();

                Parallel.ForEach(dropsToRemove, drop =>
                {
                    Broadcast(drop.GenerateOut(drop.TransportId));
                    DroppedList.TryRemove(drop.TransportId, out MapItem value);
                });
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        public void RemoveMonster(MapMonster monsterToRemove)
        {
            _monsters.TryRemove(monsterToRemove.MapMonsterId, out MapMonster value);
        }

        public void SpawnButton(MapButton parameter)
        {
            Buttons.Add(parameter);
            Broadcast(parameter.GenerateIn());
        }

        public void DespawnMonster(int monsterVnum)
        {
            Parallel.ForEach(_monsters.Select(s => s.Value).Where(s => s.MonsterVNum == monsterVnum), monster =>
            {
                monster.GenerateDeath();
                Broadcast(monster.GenerateOut());
            });
        }

        public void DespawnMonster(MapMonster monster)
        {
            monster.GenerateDeath();
            Broadcast(monster.GenerateOut());
        }

        internal void CreatePortal(Portal portal, int timeInSeconds = 0, bool isTemporary = false)
        {
            portal.SourceMapInstanceId = MapInstanceId;
            Portals.Add(portal);
            Broadcast(portal.GenerateGp());
            if (isTemporary)
            {
                Observable.Timer(TimeSpan.FromSeconds(timeInSeconds)).Subscribe(o =>
                {
                    Portals.Remove(portal);
                    MapClear();
                });
            }
        }

        internal IEnumerable<Character> GetCharactersInRange(short mapX, short mapY, byte distance)
        {
            List<Character> characters = new List<Character>();
            IEnumerable<ClientSession> cl = Sessions.Where(s => s.HasSelectedCharacter && s.Character.Hp > 0);
            IEnumerable<ClientSession> clientSessions = cl as IList<ClientSession> ?? cl.ToList();
            for (int i = clientSessions.Count() - 1; i >= 0; i--)
            {
                if (Map.GetDistance(new MapCell { X = mapX, Y = mapY }, new MapCell { X = clientSessions.ElementAt(i).Character.PositionX, Y = clientSessions.ElementAt(i).Character.PositionY }) <=
                    distance + 1)
                {
                    characters.Add(clientSessions.ElementAt(i).Character);
                }
            }
            return characters;
        }

        public IEnumerable<IBattleEntity> GetBattleEntitiesInRange(MapCell pos, byte distance)
        {
            return BattleEntities.Where(b => Map.GetDistance(b.GetPos(), pos) <= distance);
        }

        internal void RemoveMonstersTarget(object target)
        {
            Parallel.ForEach(Monsters.Where(m => m.Target == target), monster => { monster.RemoveTarget(); });
        }

        public void ThrowItems(Tuple<int, short, byte, int, int> parameter)
        {
            MapMonster mon = Monsters.FirstOrDefault(s => s.MapMonsterId == parameter.Item1);

            if (mon == null)
            {
                return;
            }
            short originX = mon.MapX;
            short originY = mon.MapY;
            int amount = ServerManager.Instance.RandomNumber(parameter.Item4, parameter.Item5);
            if (parameter.Item2 == 1024)
            {
                amount *= ServerManager.Instance.GoldRate;
            }
            for (int i = 0; i < parameter.Item3; i++)
            {
                positionRandomizer:
                short destX = (short)(originX + ServerManager.Instance.RandomNumber(-10, 10));
                short destY = (short)(originY + ServerManager.Instance.RandomNumber(-10, 10));
                if (Map.IsBlockedZone(destX, destY))
                {
                    goto positionRandomizer;
                }
                MonsterMapItem droppedItem = new MonsterMapItem(destX, destY, parameter.Item2, amount);
                DroppedList[droppedItem.TransportId] = droppedItem;
                Broadcast(
                    $"throw {droppedItem.ItemVNum} {droppedItem.TransportId} {originX} {originY} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)}");
            }
        }

        private void StartLife()
        {
            Life = Observable.Interval(TimeSpan.FromSeconds(1)).Subscribe(x =>
            {
                WaveEvents.ToList().ForEach(s =>
                {
                    if (s.LastStart.AddSeconds(s.Delay) > DateTime.Now)
                    {
                        return;
                    }
                    if (s.Offset == 0 && Sessions.Any())
                    {
                        s.Events.ToList().ForEach(e => EventHelper.Instance.RunEvent(e));
                    }
                    s.Offset = s.Offset > 0 ? (byte)(s.Offset - 1) : (byte)0;
                    s.LastStart = DateTime.Now;
                });
                try
                {
                    if (IsSleeping)
                    {
                        return;
                    }
                    if (Monsters.Count(s => s.IsAlive) == 0)
                    {
                        OnMapClean.ToList().ForEach(e => { EventHelper.Instance.RunEvent(e); });
                        OnMapClean.Clear();
                    }

                    Parallel.ForEach(Monsters.Where(s => s.Life == null), m => { m.StartLife(); });
                    Parallel.ForEach(Npcs.Where(s => s.Life == null), m => { m.StartLife(); });
                    RemoveMapItem();
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            });
        }

        internal void SummonMonsters(IEnumerable<ToSummon> summonParameters)
        {
            // TODO: Parallelize, if possible.
            foreach (ToSummon mon in summonParameters)
            {
                NpcMonster npcmonster = ServerManager.Instance.GetNpc(mon.VNum);
                if (npcmonster == null || ServerManager.Instance.RandomNumber() > mon.SummonChance)
                {
                    continue;
                }
                MapMonster monster = new MapMonster
                {
                    MonsterVNum = npcmonster.NpcMonsterVNum,
                    MapY = mon.SpawnCell.Y,
                    MapX = mon.SpawnCell.X,
                    MapId = Map.MapId,
                    IsMoving = mon.IsMoving,
                    MapMonsterId = GetNextMonsterId(),
                    ShouldRespawn = false,
                    Target = mon.Target,
                    IsTarget = mon.IsTarget,
                    OnNoticeEvents = mon.NoticingEvents,
                    IsBonus = mon.IsBonusOrProtected,
                    IsBoss = mon.IsBonusOrProtected
                };
                monster.Initialize(this);
                monster.IsHostile = mon.IsHostile;
                monster.BattleEntity.OnDeathEvents = mon.DeathEvents;
                AddMonster(monster);
                Broadcast(monster.GenerateIn());
            }
        }

        internal void SummonNpcs(IEnumerable<ToSummon> summonParameters)
        {
            // TODO: Parallelize, if possible.
            foreach (ToSummon mon in summonParameters)
            {
                NpcMonster npcmonster = ServerManager.Instance.GetNpc(mon.VNum);
                if (npcmonster == null || ServerManager.Instance.RandomNumber() > mon.SummonChance)
                {
                    continue;
                }
                MapNpc npc = new MapNpc
                {
                    NpcVNum = npcmonster.NpcMonsterVNum,
                    MapY = mon.SpawnCell.X,
                    MapX = mon.SpawnCell.Y,
                    MapId = Map.MapId,
                    IsHostile = true,
                    IsMoving = true,
                    MapNpcId = GetNextNpcId(),
                    Target = mon.Target,
                    IsMate = mon.IsBossOrMate,
                    IsProtected = mon.IsBonusOrProtected
                };
                npc.Initialize(this);
                npc.BattleEntity.OnDeathEvents = mon.DeathEvents;
                AddNpc(npc);
                Broadcast(npc.GenerateIn());
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            Clock.Dispose();
            Life.Dispose();
            _monsters.Select(s => s.Value).ToList().ForEach(monster => monster.Life?.Dispose());
            _npcs.Select(s => s.Value).ToList().ForEach(npc => npc.Life.Dispose());

            foreach (ClientSession session in ServerManager.Instance.Sessions.Where(s => s.Character != null && s.Character.MapInstanceId == MapInstanceId))
            {
                ServerManager.Instance.ChangeMap(session.Character.CharacterId, session.Character.MapId, session.Character.MapX, session.Character.MapY);
            }
        }

        #endregion

        #region Singleton

        private static EventHelper _instance;

        public static EventHelper Instance
        {
            get { return _instance ?? (_instance = new EventHelper()); }
        }

        #endregion
    }
}