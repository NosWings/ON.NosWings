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
using System.Threading;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using System.Threading.Tasks;

namespace OpenNos.GameObject.Event.INSTANTBATTLE
{
    public static class InstantBattle
    {
        #region Methods

        public async static void GenerateInstantBattle(bool useTimer = true)
        {
            if (useTimer)
            {
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 5), 1));
                await Task.Delay(4 * 60 * 1000);
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES"), 1), 1));
                await Task.Delay(30 * 1000);
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 30), 1));
                await Task.Delay(20 * 1000);
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 0));
                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                    string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS"), 10), 1));
                await Task.Delay(10 * 1000);
            }

            ServerManager.Instance.Broadcast(
                UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"),
                    1));
            ServerManager.Instance.Broadcast(
                $"qnaml 1 #guri^506 {Language.Instance.GetMessageFromKey("INSTANTBATTLE_QUESTION")}");
            ServerManager.Instance.EventInWaiting = true;
            await Task.Delay(30 * 1000);
            ServerManager.Instance.Broadcast(
                UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_STARTED"),
                    1));
            ServerManager.Instance.Sessions.Where(s => s.Character != null && !s.Character.IsWaitingForEvent).ToList()
                .ForEach(s => s.SendPacket("esf"));
            ServerManager.Instance.EventInWaiting = false;
            IEnumerable<ClientSession> sessions = ServerManager.Instance.Sessions.Where(s =>
                s.Character != null && s.Character.IsWaitingForEvent &&
                s.CurrentMapInstance?.MapInstanceType == MapInstanceType.BaseMapInstance);
            List<Tuple<MapInstance, byte>> maps = new List<Tuple<MapInstance, byte>>();
            MapInstance map = null;
            int i = -1;
            int level = 0;
            byte instancelevel = 1;
            foreach (ClientSession s in sessions.OrderBy(s => s.Character?.Level))
            {
                i++;
                if (s.Character.Level > 79 && level <= 79)
                {
                    i = 0;
                    instancelevel = 80;
                }
                else if (s.Character.Level > 69 && level <= 69)
                {
                    i = 0;
                    instancelevel = 70;
                }
                else if (s.Character.Level > 59 && level <= 59)
                {
                    i = 0;
                    instancelevel = 60;
                }
                else if (s.Character.Level > 49 && level <= 49)
                {
                    i = 0;
                    instancelevel = 50;
                }
                else if (s.Character.Level > 39 && level <= 39)
                {
                    i = 0;
                    instancelevel = 30;
                }

                if ((i % 50) == 0)
                {
                    map = ServerManager.Instance.GenerateMapInstance(2004, MapInstanceType.NormalInstance,
                        new InstanceBag());
                    maps.Add(new Tuple<MapInstance, byte>(map, instancelevel));
                }

                if (map != null)
                {
                    ServerManager.Instance.TeleportOnRandomPlaceInMap(s, map.MapInstanceId);
                }

                level = s.Character.Level;
            }

            ServerManager.Instance.Sessions.Where(s => s.Character != null).ToList()
                .ForEach(s => s.Character.IsWaitingForEvent = false);
            foreach (Tuple<MapInstance, byte> mapinstance in maps)
            {
                ServerManager.Instance.StartedEvents.Remove(EventType.INSTANTBATTLE);
                await Task.Delay(10 * 1000);
                if (mapinstance.Item1.Sessions.Count() < 3)
                {
                    mapinstance.Item1.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                        Language.Instance.GetMessageFromKey("INSTANTBATTLE_NOT_ENOUGH_PLAYERS"), 0));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(5),
                        new EventContainer(mapinstance.Item1, EventActionType.DISPOSEMAP, null));
                    continue;
                }

                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(12), new EventContainer(mapinstance.Item1,
                    EventActionType.REGISTEREVENT,
                    new Tuple<string, ConcurrentBag<EventContainer>>("OnMapClean",
                        new ConcurrentBag<EventContainer>
                        {
                            new EventContainer(mapinstance.Item1, EventActionType.INSTANTBATLLEREWARDS, null)
                        })));

                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(15),
                    new EventContainer(mapinstance.Item1, EventActionType.DISPOSEMAP, null));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(3),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 12),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(5),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 10),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(10),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 5),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(11),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 4),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(12),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 3),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(13),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 2),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_MINUTES_REMAINING"), 1),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(14.5),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SECONDS_REMAINING"), 30),
                            0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromMinutes(0),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(7),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_APPEAR"), 0)));
                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(3),
                    new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));

                for (int wave = 0; wave < 4; wave++)
                {
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(130 + wave * 160), new EventContainer(
                        mapinstance.Item1, EventActionType.SENDPACKET,
                        UserInterfaceHelper.Instance.GenerateMsg(
                            Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_WAVE"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(160 + wave * 160),
                        new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                            UserInterfaceHelper.Instance.GenerateMsg(
                                Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_INCOMING"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(170 + wave * 160),
                        new EventContainer(mapinstance.Item1, EventActionType.SENDPACKET,
                            UserInterfaceHelper.Instance.GenerateMsg(
                                Language.Instance.GetMessageFromKey("INSTANTBATTLE_MONSTERS_HERE"), 0)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(10 + wave * 160),
                        new EventContainer(mapinstance.Item1, EventActionType.SPAWNMONSTERS,
                            GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, wave)));
                    EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(140 + wave * 160),
                        new EventContainer(mapinstance.Item1, EventActionType.DROPITEMS,
                            GetInstantBattleDrop(mapinstance.Item1.Map, mapinstance.Item2, wave)));
                }

                EventHelper.Instance.ScheduleEvent(TimeSpan.FromSeconds(650),
                    new EventContainer(mapinstance.Item1, EventActionType.SPAWNMONSTERS,
                        GetInstantBattleMonster(mapinstance.Item1.Map, mapinstance.Item2, 4)));
            }
        }

        private static IEnumerable<Tuple<short, int, short, short>> GenerateDrop(Map.Map map, short vnum,
            int amountofdrop, int amount)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            for (int i = 0; i < amountofdrop; i++)
            {
                MapCell cell = map.GetRandomPosition();
                dropParameters.Add(new Tuple<short, int, short, short>(vnum, amount, cell.X, cell.Y));
            }

            return dropParameters;
        }

        private static List<Tuple<short, int, short, short>> GetInstantBattleDrop(Map.Map map, short instantbattletype,
            int wave)
        {
            List<Tuple<short, int, short, short>> dropParameters = new List<Tuple<short, int, short, short>>();
            switch (instantbattletype)
            {
                case 1:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                500 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 2027, 8, 5));
                            dropParameters.AddRange(GenerateDrop(map, 2018, 5, 5));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                1000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1002, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1005, 16, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                1500 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1002, 10, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1005, 10, 5));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                2000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1003, 10, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1006, 10, 5));
                            break;
                    }

                    break;

                case 40:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                1500 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 5, 3));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                2000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                2500 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                3000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 5, 1));
                            break;
                    }

                    break;

                case 50:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                1500 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 5, 3));
                            dropParameters.AddRange(GenerateDrop(map, 180, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                2000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1008, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 181, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                2500 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                3000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1009, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 5, 1));
                            break;
                    }

                    break;

                case 60:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                3000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 8, 4));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                4000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 10, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 5, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                5000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 10, 13));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 8, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 8, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                7000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1029, 5, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 13, 1));
                            break;
                    }

                    break;

                case 70:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                3000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 8, 3));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 5, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                4000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 15, 4));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 10, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                5000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1010, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 13, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 13, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                7000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 13, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1248, 13, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1029, 5, 1));
                            break;
                    }

                    break;

                case 80:
                    switch (wave)
                    {
                        case 0:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                10000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 15, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 15, 1));
                            break;

                        case 1:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                12000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 15, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 15, 1));
                            break;

                        case 2:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 15,
                                15000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 20, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1246, 15, 1));
                            dropParameters.AddRange(GenerateDrop(map, 1247, 15, 1));
                            break;

                        case 3:
                            dropParameters.AddRange(GenerateDrop(map, 1046, 30,
                                20000 * ServerManager.Instance.GoldRate / 4));
                            dropParameters.AddRange(GenerateDrop(map, 1011, 30, 5));
                            dropParameters.AddRange(GenerateDrop(map, 1030, 30, 1));
                            dropParameters.AddRange(GenerateDrop(map, 2282, 12, 3));
                            break;
                    }

                    break;
            }

            return dropParameters;
        }

        private static ConcurrentBag<ToSummon> GetInstantBattleMonster(Map.Map map, short instantbattletype, int wave)
        {
            ConcurrentBag<ToSummon> summonParameters = new ConcurrentBag<ToSummon>();

            switch (instantbattletype)
            {
                case 1:
                    switch (wave)
                    {
                        case 0:
                            map.GenerateSummons(1, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(58, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(105, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(107, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(108, 8, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(111, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(136, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 1:
                            map.GenerateSummons(194, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(114, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(99, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(39, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(2, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 2:
                            map.GenerateSummons(140, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(100, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(81, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(12, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(4, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 3:
                            map.GenerateSummons(115, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(112, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(110, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(14, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(5, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 4:
                            map.GenerateSummons(979, 1, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(167, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(137, 10, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(22, 15, false, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(17, 8, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(16, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;
                    }

                    break;

                case 30:
                    switch (wave)
                    {
                        case 0:
                            map.GenerateSummons(120, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(151, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(149, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(139, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(73, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 1:
                            map.GenerateSummons(152, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(147, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(104, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(62, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(8, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 2:
                            map.GenerateSummons(153, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(132, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(86, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(76, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(68, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 3:
                            map.GenerateSummons(134, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(91, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(133, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(70, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(89, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 4:
                            map.GenerateSummons(154, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(200, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(77, 8, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(217, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(724, 1, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;
                    }

                    break;

                case 50:
                    switch (wave)
                    {
                        case 0:
                            map.GenerateSummons(134, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(91, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(89, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(77, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(71, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 1:
                            map.GenerateSummons(217, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(200, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(154, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(92, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(79, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 2:
                            map.GenerateSummons(235, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(226, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(214, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(204, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(201, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 3:
                            map.GenerateSummons(249, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(236, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(227, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(218, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(202, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 4:
                            map.GenerateSummons(583, 1, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(400, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(255, 8, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(253, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(251, 10, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(205, 14, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;
                    }

                    break;

                case 60:
                    switch (wave)
                    {
                        case 0:
                            map.GenerateSummons(242, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(234, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(215, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(207, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(202, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 1:
                            map.GenerateSummons(402, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(253, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(237, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(216, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(205, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 2:
                            map.GenerateSummons(402, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(243, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(228, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(255, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(205, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 3:
                            map.GenerateSummons(268, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(255, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(254, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(174, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(172, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 4:
                            map.GenerateSummons(725, 1, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(407, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(272, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(261, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(256, 12, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(275, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;
                    }

                    break;

                case 70:
                    switch (wave)
                    {
                        case 0:
                            map.GenerateSummons(402, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(253, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(237, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(216, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(205, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 1:
                            map.GenerateSummons(402, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(243, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(228, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(225, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(205, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 2:
                            map.GenerateSummons(255, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(254, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(251, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(174, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(172, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 3:
                            map.GenerateSummons(407, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(272, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(261, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(257, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(256, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 4:
                            map.GenerateSummons(748, 1, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(444, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(439, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(275, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(274, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(273, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(163, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;
                    }

                    break;

                case 80:
                    switch (wave)
                    {
                        case 0:
                            map.GenerateSummons(1007, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1003, 15, false, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1002, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1001, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1000, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 1:
                            map.GenerateSummons(1199, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1198, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1197, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1196, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1123, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 2:
                            map.GenerateSummons(1305, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1304, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1303, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1302, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1194, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 3:
                            map.GenerateSummons(1902, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1901, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1900, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1045, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1043, 15, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1042, 16, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;

                        case 4:
                            map.GenerateSummons(637, 1, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1903, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1053, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1051, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1049, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1048, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            map.GenerateSummons(1047, 13, true, new ConcurrentBag<EventContainer>()).ToList()
                                .ForEach(s => summonParameters.Add(s));
                            break;
                    }

                    break;
            }

            return summonParameters;
        }

        #endregion
    }
}