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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.ACT4
{
    public class Act4Ship
    {
        public static void AddNpc(FactionType faction)
        {
            var leikaNpc = new MapNpc
            {
                NpcVNum = 540,
                MapNpcId = faction == FactionType.Angel
                    ? ServerManager.Instance.Act4ShipAngel.GetNextId()
                    : ServerManager.Instance.Act4ShipDemon.GetNextId(),
                Dialog = 433,
                MapId = 149,
                MapX = 31,
                MapY = 28,
                IsMoving = false,
                Position = 3,
                IsSitting = false
            };

            leikaNpc.Initialize(ServerManager.Instance.Act4ShipDemon);
            leikaNpc.Initialize(ServerManager.Instance.Act4ShipAngel);
            ServerManager.Instance.Act4ShipDemon.AddNpc(leikaNpc);
            ServerManager.Instance.Act4ShipAngel.AddNpc(leikaNpc);
        }

        public static DateTime RoundUp(DateTime dt, TimeSpan d) => new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks);

        public static void GenerateAct4Ship(FactionType faction)
        {
            AddNpc(faction);
            EventHelper.Instance.RunEvent(new EventContainer(
                ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(145)),
                EventActionType.NPCSEFFECTCHANGESTATE, true));
            DateTime result = RoundUp(DateTime.Now, TimeSpan.FromMinutes(5));
            Observable.Timer(result - DateTime.Now).Subscribe(x => Act4ShipTask.Run(faction));
        }
    }

    public static class Act4ShipTask
    {
        #region Methods

        public static void Run(FactionType faction)
        {
            MapInstance map = faction == FactionType.Angel
                ? ServerManager.Instance.Act4ShipAngel
                : ServerManager.Instance.Act4ShipDemon;
            while (true)
            {
                OpenShip();
                Thread.Sleep(60 * 1000);
                map.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("SHIP_MINUTES"), 4), 0));
                Thread.Sleep(60 * 1000);
                map.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("SHIP_MINUTES"), 3), 0));
                Thread.Sleep(60 * 1000);
                map.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("SHIP_MINUTES"), 2), 0));
                Thread.Sleep(60 * 1000);
                map.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHIP_MINUTE"), 0));
                LockShip();
                Thread.Sleep(30 * 1000);
                map.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("SHIP_SECONDS"), 30), 0));
                Thread.Sleep(20 * 1000);
                map.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("SHIP_SECONDS"), 10), 0));
                Thread.Sleep(10 * 1000);
                map.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHIP_SETOFF"), 0));
                Thread.Sleep(3 * 1000);
                List<ClientSession> sessions = map.Sessions.Where(s => s?.Character != null).ToList();
                TeleportPlayers(sessions);
            }
        }

        private static void TeleportPlayers(IEnumerable<ClientSession> sessions)
        {
            foreach (ClientSession s in sessions)
            {
                s.Character.Gold -= 3000;
                switch (s.Character.Faction)
                {
                    case FactionType.Neutral:
                        ServerManager.Instance.ChangeMap(s.Character.CharacterId, 145, 51, 41);
                        s.SendPacket(UserInterfaceHelper.Instance.GenerateInfo("NEED_FACTION_ACT4"));
                        return;
                    case FactionType.Angel:
                        s.Character.MapId = 130;
                        s.Character.MapX = (short)(12 + ServerManager.Instance.RandomNumber(-2, 3));
                        s.Character.MapY = (short)(40 + ServerManager.Instance.RandomNumber(-2, 3));
                        break;
                    case FactionType.Demon:
                        s.Character.MapId = 131;
                        s.Character.MapX = (short)(12 + ServerManager.Instance.RandomNumber(-2, 3));
                        s.Character.MapY = (short)(40 + ServerManager.Instance.RandomNumber(-2, 3));
                        break;
                }

                //todo: get act4 channel dynamically
                if (!s.Character.ConnectAct4())
                {
                    s.Character.Gold += 3000;
                    ServerManager.Instance.ChangeMap(s.Character.CharacterId, 145, 51, 41);
                }
            }
        }

        private static void LockShip()
        {
            EventHelper.Instance.RunEvent(new EventContainer(
                ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(145)),
                EventActionType.NPCSEFFECTCHANGESTATE, false));
        }

        private static void OpenShip()
        {
            EventHelper.Instance.RunEvent(new EventContainer(
                ServerManager.Instance.GetMapInstance(ServerManager.Instance.GetBaseMapInstanceIdByMapId(145)),
                EventActionType.NPCSEFFECTCHANGESTATE, true));
        }

        #endregion
    }
}