using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace OpenNos.GameObject.Event
{
    public static class Act4Raid
    {

        #region Methods

        public static void GenerateRaid(Act4RaidType type, byte faction)
        {
            Act4RaidThread raidThread = new Act4RaidThread();
            raidThread.Run(type, faction);
        }

        #endregion
    }


    public class Act4RaidThread
    {
        #region Properties

        private const int Interval = 30;

        private const int BossSpawn = 30 * 60;

        private int _raidTime = 60 * 60;

        private Act4RaidType _type;

        private byte _faction;
        private short _bossPortalX;
        private short _bossPortalY;
        private short _bossPortalToX = -1;
        private short _bossPortalToY = -1;

        #endregion

        #region Methods

        public void Run(Act4RaidType type, byte faction)
        {
            ConcurrentBag<MonsterToSummon> bossParametter = new ConcurrentBag<MonsterToSummon>();

            _type = type;
            _faction = faction;
            short raidMap = 0;
            short boxVnum = 0;
            short destX = 0;
            short destY = 0;

            #region SetParameters

            switch (_type)
            {
                case Act4RaidType.Morcos:
                    bossParametter.Add(new MonsterToSummon(563, new MapCell { X = 56, Y = 11}, -1, false) { DeathEvents = new List<EventContainer>() });
                    raidMap = 135;
                    boxVnum = 882;
                    destX = 151;
                    destY = 45;
                    _bossPortalX = 40;
                    _bossPortalY = 177;
                    _bossPortalToX = 55;
                    _bossPortalToY = 80;
                    break;
                case Act4RaidType.Hatus:
                    bossParametter.Add(new MonsterToSummon(282, new MapCell { X = 36, Y = 18 }, -1, false) { DeathEvents = new List<EventContainer>() });
                    raidMap = 137;
                    boxVnum = 185;
                    destX = 14;
                    destY = 6;
                    _bossPortalX = 37;
                    _bossPortalY = 157;
                    _bossPortalToX = 35;
                    _bossPortalToY = 59;
                    break;
                case Act4RaidType.Calvina:
                    bossParametter.Add(new MonsterToSummon(629, new MapCell { X = 26, Y = 25 }, -1, true) { DeathEvents = new List<EventContainer>() });
                    raidMap = 139;
                    boxVnum = 942;
                    destX = 0;
                    destY = 74;
                    _bossPortalX = 201;
                    _bossPortalY = 93;
                    break;
                case Act4RaidType.Berios:
                    bossParametter.Add(new MonsterToSummon(624, new MapCell { X = 30, Y = 28 }, -1, true) { DeathEvents = new List<EventContainer>() });
                    raidMap = 141;
                    boxVnum = 999;
                    destX = 17;
                    destY = 17;
                    _bossPortalX = 188;
                    _bossPortalY = 97;
                    break;
            }

            #endregion

            ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId((short)(129 + _faction))).CreatePortal(new Portal()
            {
                SourceMapId = (short)(129 + _faction),
                SourceX = 53,
                SourceY = 53,
                DestinationMapId = 0,
                DestinationX = destX,
                DestinationY = destY,
                Type = (short)(9 + _faction)
            });

            ServerManager.Instance.Act4Maps.ForEach(m => m.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ACT4_RAID_OPEN"), type.ToString()), 0)));

            while (_raidTime > 0)
            {
                RefreshAct4Raid(_raidTime, raidMap, destX, destY);

                if (_raidTime == BossSpawn)
                {
                    SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
                    foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
                    {
                        if (fam.Act4Raid == null || fam.Act4RaidBossMap == null)
                        {
                            continue;
                        }
                        SpawnBoss(fam.Act4Raid, fam.Act4RaidBossMap, bossParametter, boxVnum);
                    }
                }

                _raidTime -= Interval;
                Thread.Sleep(Interval * 1000);
            }
            EndRaid();
        }

        private void EndRaid()
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
            {
                if (fam.Act4Raid == null)
                {
                    continue;
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4Raid, EventActionType.DISPOSEMAP, null));
                fam.Act4Raid = null;

                if (fam.Act4RaidBossMap == null)
                {
                    continue;
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4RaidBossMap, EventActionType.DISPOSEMAP, null));
                fam.Act4RaidBossMap = null;
            }
            if (_faction == 1)
            { ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(130)).Portals.RemoveAll(s => s.Type.Equals(10)); }
            else
            { ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(131)).Portals.RemoveAll(s => s.Type.Equals(11)); }
        }

        private void RefreshAct4Raid(int remaining, short raidMap, short sourceX, short sourceY)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
            {
                if (fam.Act4Raid == null)
                {
                    fam.Act4Raid = ServerManager.Instance.GenerateMapInstance(raidMap, MapInstanceType.RaidInstance, new InstanceBag());
                    fam.Act4Raid.CreatePortal(new Portal()
                    {
                        SourceMapId = fam.Act4Raid.Map.MapId,
                        SourceX = sourceX,
                        SourceY = sourceY,
                        DestinationMapId = (short) (129 + _faction),
                        DestinationX = 53,
                        DestinationY = 53,
                        Type = -1
                    });
                    fam.Act4Raid.MapIndexX = (byte) sourceX;
                    fam.Act4Raid.MapIndexY = (byte) sourceY;

                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4Raid, EventActionType.CLOCK, remaining * 10));
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4Raid, EventActionType.STARTCLOCK,
                    new Tuple<ConcurrentBag<EventContainer>, ConcurrentBag<EventContainer>>(new ConcurrentBag<EventContainer>(),
                        new ConcurrentBag<EventContainer>())));

                if (fam.Act4RaidBossMap == null)
                {
                    fam.Act4RaidBossMap = ServerManager.Instance.GenerateMapInstance(raidMap++, MapInstanceType.RaidInstance, new InstanceBag());
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4RaidBossMap, EventActionType.CLOCK, remaining * 10));
                EventHelper.Instance.RunEvent(new EventContainer(fam.Act4RaidBossMap, EventActionType.STARTCLOCK,
                    new Tuple<ConcurrentBag<EventContainer>, ConcurrentBag<EventContainer>>(new ConcurrentBag<EventContainer>(),
                        new ConcurrentBag<EventContainer>())));
            }
        }

        private void SpawnBoss(MapInstance raidMap, MapInstance raidBossMap, ConcurrentBag<MonsterToSummon> summonParameters, short boxVnum)
        {
            Portal bossPortal = new Portal
            {
                PortalId = 1,
                SourceX = _bossPortalX,
                SourceY = _bossPortalY,
                Type = 0,
                DestinationX = _bossPortalToX,
                DestinationY = _bossPortalToY,
                DestinationMapId = raidBossMap.Map.MapId,
                SourceMapInstanceId = raidMap.MapInstanceId,
                DestinationMapInstanceId = raidBossMap.MapInstanceId,
            };

            EventHelper.Instance.RunEvent(new EventContainer(raidBossMap, EventActionType.SPAWNMONSTERS, summonParameters));
            EventHelper.Instance.RunEvent(new EventContainer(raidMap, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BOSS_APPEAR"), 0)));
            EventHelper.Instance.RunEvent(new EventContainer(raidMap, EventActionType.SPAWNPORTAL, bossPortal));
            EventHelper.Instance.RunEvent(new EventContainer(raidBossMap, EventActionType.REGISTERWAVE, new EventWave(90, GetWaveMonster(raidBossMap))));


            MapMonster boss = raidBossMap.Monsters.FirstOrDefault(m => m.MonsterVNum == summonParameters.FirstOrDefault()?.VNum);

            if (boss == null)
            {
                return;
            }
            //Throw Gold
            boss.OnDeathEvents.Add(new EventContainer(raidBossMap, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int>(-1, 1046, 30, 20000, 20001)));
            //RaidBox
            boss.OnDeathEvents.Add(new EventContainer(raidBossMap, EventActionType.MAPGIVE, new Tuple<bool, short, byte, short>(true, boxVnum, 1, 50)));
            boss.OnDeathEvents.Add(new EventContainer(raidMap, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BOSS_DIED"), 0)));
            boss.OnDeathEvents.Add(new EventContainer(raidMap, EventActionType.REMOVEPORTAL, bossPortal.PortalId));
            boss.OnDeathEvents.Add(new EventContainer(raidBossMap, EventActionType.ACT4RAIDEND, new Tuple<MapInstance, short, short>(raidMap, raidMap.MapIndexX, raidMap.MapIndexY)));

        }

        private ConcurrentBag<EventContainer> GetWaveMonster(MapInstance mapInstance)
        {
            ConcurrentBag<MonsterToSummon> summonParameters = new ConcurrentBag<MonsterToSummon>();
            ConcurrentBag<EventContainer> evtParameters = new ConcurrentBag<EventContainer>();

            switch (_type)
            {
                case Act4RaidType.Berios:
                    mapInstance.Map.GenerateMonsters(780, 1, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    mapInstance.Map.GenerateMonsters(781, 1, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    mapInstance.Map.GenerateMonsters(782, 2, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    mapInstance.Map.GenerateMonsters(783, 2, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    break;

                case Act4RaidType.Calvina:
                    mapInstance.Map.GenerateMonsters(770, 3, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    mapInstance.Map.GenerateMonsters(771, 3, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    break;

                case Act4RaidType.Hatus:
                    mapInstance.Map.GenerateMonsters(574, 2, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    mapInstance.Map.GenerateMonsters(575, 2, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    mapInstance.Map.GenerateMonsters(576, 2, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    break;

                case Act4RaidType.Morcos:
                    mapInstance.Map.GenerateMonsters(561, 3, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    mapInstance.Map.GenerateMonsters(562, 3, true, new List<EventContainer>()).ToList().ForEach(s => summonParameters.Add(s));
                    break;
            }

            foreach (MonsterToSummon sParameters in summonParameters)
            {
                evtParameters.Add(new EventContainer(mapInstance, EventActionType.SPAWNMONSTERS, sParameters));
            }

            return evtParameters;
        }

        #endregion
    }
}
