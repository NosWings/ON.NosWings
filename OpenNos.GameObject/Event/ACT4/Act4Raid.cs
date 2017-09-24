using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;


namespace OpenNos.GameObject.Event
{
    public static class Act4Raid
    {

        #region Methods

        public static void GenerateRaid(Act4RaidType type, byte faction)
        {
            ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId((short)(129 + faction))).CreatePortal(new Portal()
            {
                SourceMapId = (short)(129 + faction),
                SourceX = 53,
                SourceY = 53,
                DestinationMapId = 0,
                DestinationX = 1,
                DestinationY = 1,
                Type = (short)(9 + faction)
            });

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

        private int RaidTime = 60 * 60;

        #endregion

        #region Methods

        public void Run(Act4RaidType type, byte faction)
        {
            ConcurrentBag<MonsterToSummon> bossParametter = new ConcurrentBag<MonsterToSummon>();
            List<EventContainer> deathEvents = new List<EventContainer>();

            short raidMap = 0;
            short boxVnum = 0;

            switch (type)
            {
                case Act4RaidType.Morcos:
                    bossParametter.Add(new MonsterToSummon(563, new MapCell { X = 56, Y = 11}, -1, false, isBoss: true ) { DeathEvents = deathEvents });
                    raidMap = 135;
                    boxVnum = 882;
                    break;
                case Act4RaidType.Hatus:
                    bossParametter.Add(new MonsterToSummon(282, new MapCell { X = 36, Y = 18 }, -1, false, isBoss: true) { DeathEvents = deathEvents });
                    raidMap = 137;
                    boxVnum = 185;
                    break;
                case Act4RaidType.Calvina:
                    bossParametter.Add(new MonsterToSummon(629, new MapCell { X = 26, Y = 25 }, -1, true, isBoss: true) { DeathEvents = deathEvents });
                    raidMap = 139;
                    boxVnum = 942;
                    break;
                case Act4RaidType.Berios:
                    bossParametter.Add(new MonsterToSummon(624, new MapCell { X = 30, Y = 28 }, -1, true, isBoss: true) { DeathEvents = deathEvents });
                    raidMap = 141;
                    boxVnum = 999;
                    break;
            }

            ServerManager.Instance.Act4Maps.ForEach(m => m.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("ACT4_RAID_OPEN"), type.ToString()), 0)));

            while (RaidTime > 0)
            {
                RefreshAct4Raid(RaidTime, raidMap);

                if (RaidTime == BossSpawn)
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

                RaidTime -= Interval;
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
            ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(130)).Portals.RemoveAll(s => s.Type.Equals(10));
            ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(131)).Portals.RemoveAll(s => s.Type.Equals(11));
        }

        private void RefreshAct4Raid(int remaining, short raidMap)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            foreach (Family fam in ServerManager.Instance.FamilyList.ToArray())
            {
                if (fam.Act4Raid == null)
                {
                    fam.Act4Raid = ServerManager.Instance.GenerateMapInstance(raidMap, MapInstanceType.RaidInstance, new InstanceBag());
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
            MapMonster boss = raidBossMap.Monsters.FirstOrDefault(m => m.IsBoss);

            if (boss == null)
            {
                return;
            }
            //Gold
            boss.OnDeathEvents.Add(new EventContainer(boss.MapInstance, EventActionType.THROWITEMS, new Tuple<int, short, byte, int, int>(-1, 1046, 30, 15000, 20000)));
            boss.OnDeathEvents.Add(new EventContainer(boss.MapInstance, EventActionType.MAPGIVE, new Tuple<bool, short, byte, short>(true, boxVnum, 1, 50)));
        }

        #endregion
    }
}
