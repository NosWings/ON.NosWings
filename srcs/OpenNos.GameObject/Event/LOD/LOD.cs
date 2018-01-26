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
using System.Reactive.Linq;
using System.Threading;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.DAL.EF;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.LOD
{
    public class Lod
    {
        #region Methods

        public static void GenerateLod(int lodtime = 60)
        {
            const int hornTime = 30;
            const int hornRepawn = 4;
            const int hornStay = 1;
            EventHelper.Instance.RunEvent(new EventContainer(ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(98)), EventActionType.NPCSEFFECTCHANGESTATE, true));
            LodThread lodThread = new LodThread();
            Observable.Timer(TimeSpan.FromMinutes(0)).Subscribe(x => lodThread.Run(lodtime * 60, hornTime * 60, hornRepawn * 60, hornStay * 60));
            List<MapNpc> portalList = ServerManager.Instance.GetMapNpcsPerVNum(453);
            if (portalList != null)
            {
                foreach (MapNpc npc in portalList)
                {
                    npc.EffectActivated = true;
                }
            }
        }

        #endregion
    }

    public class LodThread
    {
        #region Methods

        public void Run(int lodTime, int hornTime, int hornRespawn, int hornStay)
        {
            const int interval = 30;
            int dhspawns = 0;

            while (lodTime > 0)
            {
                RefreshLod(lodTime);

                if (lodTime == hornTime || lodTime == hornTime - hornRespawn * dhspawns)
                {
                    SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
                    foreach (Family fam in ServerManager.Instance.FamilyList)
                    {
                        if (fam.LandOfDeath == null)
                        {
                            continue;
                        }
                        EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.CHANGEXPRATE, 3));
                        EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.CHANGEDROPRATE, 3));
                        SpawnDh(fam.LandOfDeath);
                    }
                }        
                else if (lodTime == hornTime - hornRespawn * dhspawns - hornStay)
                {
                    SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
                    foreach (Family fam in ServerManager.Instance.FamilyList)
                    {
                        if (fam.LandOfDeath == null)
                        {
                            continue;
                        }
                        DespawnDh(fam.LandOfDeath);
                        dhspawns++;
                    }
                }

                lodTime -= interval;
                Thread.Sleep(interval * 1000);
            }
            EndLod();
        }

        private void DespawnDh(MapInstance landOfDeath)
        {
            EventHelper.Instance.RunEvent(new EventContainer(ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(98)), EventActionType.NPCSEFFECTCHANGESTATE, false));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_DISAPEAR"), 0)));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.UNSPAWNMONSTERS, 443));
        }

        private void EndLod()
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            foreach (Family fam in ServerManager.Instance.FamilyList)
            {
                if (fam.LandOfDeath == null)
                {
                    continue;
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.DISPOSEMAP, null));
                fam.LandOfDeath = null;
            }
            ServerManager.Instance.StartedEvents.Remove(EventType.LOD);
            ServerManager.Instance.StartedEvents.Remove(EventType.LODDH);
        }

        private void RefreshLod(int remaining)
        {
            SpinWait.SpinUntil(() => !ServerManager.Instance.InFamilyRefreshMode);
            foreach (Family fam in ServerManager.Instance.FamilyList)
            {
                if (fam.LandOfDeath == null)
                {
                    fam.LandOfDeath = ServerManager.Instance.GenerateMapInstance(150, MapInstanceType.LodInstance, new InstanceBag());
                }
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.CLOCK, remaining * 10));
                EventHelper.Instance.RunEvent(new EventContainer(fam.LandOfDeath, EventActionType.STARTCLOCK,
                    new Tuple<ConcurrentBag<EventContainer>, ConcurrentBag<EventContainer>>(new ConcurrentBag<EventContainer>(),
                        new ConcurrentBag<EventContainer>())));
            }
        }

        private void SpawnDh(MapInstance landOfDeath)
        {
            List<MapNpc> portalList = ServerManager.Instance.GetMapNpcsPerVNum(453);
            if (portalList != null)
            {
                foreach (MapNpc npc in portalList)
                {
                    npc.EffectActivated = false;
                }
            }
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SPAWNONLASTENTRY, 443));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SENDPACKET, "df 2"));
            EventHelper.Instance.RunEvent(new EventContainer(landOfDeath, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("HORN_APPEAR"), 0)));
        }

        #endregion
    }
}