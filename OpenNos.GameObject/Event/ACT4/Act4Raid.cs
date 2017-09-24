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

        private byte _faction;

        #endregion

        #region Methods

        public void Run(Act4RaidType type, byte faction)
        {
            ConcurrentBag<MonsterToSummon> bossParametter = new ConcurrentBag<MonsterToSummon>();

            _faction = faction;
            short raidMap = 0;
            short boxVnum = 0;
            short destX = 0;
            short destY = 0;

            switch (type)
            {
                case Act4RaidType.Morcos:
                    bossParametter.Add(new MonsterToSummon(563, new MapCell { X = 56, Y = 11}, -1, false) { DeathEvents = new List<EventContainer>() });
                    raidMap = 135;
                    boxVnum = 882;
                    destX = 151;
                    destY = 45;
                    break;
                case Act4RaidType.Hatus:
                    bossParametter.Add(new MonsterToSummon(282, new MapCell { X = 36, Y = 18 }, -1, false) { DeathEvents = new List<EventContainer>() });
                    raidMap = 137;
                    boxVnum = 185;
                    destX = 37;
                    destY = 157;
                    break;
                case Act4RaidType.Calvina:
                    bossParametter.Add(new MonsterToSummon(629, new MapCell { X = 26, Y = 25 }, -1, true) { DeathEvents = new List<EventContainer>() });
                    raidMap = 139;
                    boxVnum = 942;
                    destX = 201;
                    destY = 93;
                    break;
                case Act4RaidType.Berios:
                    bossParametter.Add(new MonsterToSummon(624, new MapCell { X = 30, Y = 28 }, -1, true) { DeathEvents = new List<EventContainer>() });
                    raidMap = 141;
                    boxVnum = 999;
                    destX = 188;
                    destY = 97;
                    break;
            }

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
                        Type = 1
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
            EventHelper.Instance.RunEvent(new EventContainer(raidBossMap, EventActionType.SPAWNMONSTERS, summonParameters));
            EventHelper.Instance.RunEvent(new EventContainer(raidMap, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BOSS_APPEAR"), 0)));
            MapMonster boss = raidBossMap.Monsters.FirstOrDefault(m => m.MonsterVNum == summonParameters.FirstOrDefault()?.VNum);

            if (boss == null)
            {
                return;
            }
            //Throw Gold
            boss.OnDeathEvents.Add(new EventContainer(raidBossMap, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int>(-1, 1046, 30, 20000, 20001)));
            //RaidBox
            boss.OnDeathEvents.Add(new EventContainer(raidBossMap, EventActionType.MAPGIVE, new Tuple<bool, short, byte, short>(true, boxVnum, 1, 50)));

            boss.OnDeathEvents.Add(new EventContainer(raidMap, EventActionType.REMOVEPORTAL, raidMap.Portals.FirstOrDefault(p => p.Type == (byte) PortalType.TSNormal)?.PortalId));
            boss.OnDeathEvents.Add(new EventContainer(raidBossMap, EventActionType.ACT4RAIDEND, new Tuple<MapInstance, short, short>(raidMap, raidMap.MapIndexX, raidMap.MapIndexY)));

        }

        #endregion
    }
}
