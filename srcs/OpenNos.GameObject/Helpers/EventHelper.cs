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
using OpenNos.GameObject.Event;
using OpenNos.PathFinder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Core.Extensions;
using OpenNos.GameObject.Event.ACT4;
using OpenNos.GameObject.Event.ICEBREAKER;
using OpenNos.GameObject.Event.INSTANTBATTLE;
using OpenNos.GameObject.Event.LOD;
using OpenNos.GameObject.Event.MINILANDREFRESH;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Data;
using OpenNos.PathFinder.PathFinder;

namespace OpenNos.GameObject.Helpers
{
    public class EventHelper
    {
        #region Methods

        public int CalculateComboPoint(int n)
        {
            int a = 4;
            int b = 7;
            for (int i = 0; i < n; i++)
            {
                int temp = a;
                a = b;
                b = temp + b;
            }
            return a;
        }

        public void GenerateEvent(EventType type, bool useTimer = true)
        {
            if (!ServerManager.Instance.StartedEvents.Contains(type))
            {
                Task.Factory.StartNew(() =>
                {
                    ServerManager.Instance.StartedEvents.Add(type);
                    switch (type)
                    {
                        case EventType.RANKINGREFRESH:
                            ServerManager.Instance.RefreshRanking();
                            break;

                        case EventType.LOD:
                            Lod.GenerateLod();
                            break;

                        case EventType.MINILANDREFRESHEVENT:
                            MinilandRefresh.GenerateMinilandEvent();
                            break;

                        case EventType.INSTANTBATTLE:
                            InstantBattle.GenerateInstantBattle(useTimer);
                            break;
                        case EventType.TALENTARENA:
                            //ArenaEvent.GenerateTalentArena();
                            break;
                        case EventType.LODDH:
                            Lod.GenerateLod(35);
                            break;

                        case EventType.ICEBREAKER:
                            IceBreaker.GenerateIceBreaker(useTimer);
                            break;
                    }
                });
            }
        }


        public TimeSpan GetMilisecondsBeforeTime(TimeSpan time)
        {
            TimeSpan now = TimeSpan.Parse(DateTime.Now.ToString("HH:mm"));
            TimeSpan timeLeftUntilFirstRun = time - now;
            if (timeLeftUntilFirstRun.TotalHours < 0)
            {
                timeLeftUntilFirstRun += new TimeSpan(24, 0, 0);
            }
            return timeLeftUntilFirstRun;
        }

        /// <summary>
        /// Run Event
        /// </summary>
        /// <param name="evt">Event Container</param>
        /// <param name="session">Character Session that run the event</param>
        /// <param name="monster">Monster that run the event</param>
        public void RunEvent(EventContainer evt, ClientSession session = null, MapMonster monster = null)
        {
            if (session != null)
            {
                evt.MapInstance = session.CurrentMapInstance;
                switch (evt.EventActionType)
                {
                    #region EventForUser

                    case EventActionType.NPCDIALOG:
                        session.SendPacket(session.Character.GenerateNpcDialog((int)evt.Parameter));
                        break;

                    case EventActionType.SENDPACKET:
                        session.SendPacket((string)evt.Parameter);
                        break;

                    #endregion
                }
            }
            if (evt.MapInstance == null)
            {
                return;
            }
            switch (evt.EventActionType)
            {
                #region EventForUser

                case EventActionType.NPCDIALOG:
                case EventActionType.SENDPACKET:
                    if (session == null)
                    {
                        evt.MapInstance.Sessions.ToList().ForEach(e => { RunEvent(evt, e); });
                    }
                    break;

                #endregion

                #region MapInstanceEvent

                case EventActionType.REGISTEREVENT:
                    Tuple<string, ConcurrentBag<EventContainer>> even = (Tuple<string, ConcurrentBag<EventContainer>>)evt.Parameter;
                    switch (even.Item1)
                    {
                        case "OnCharacterDiscoveringMap":
                            even.Item2.ToList().ForEach(s => evt.MapInstance.OnCharacterDiscoveringMapEvents.Add(new Tuple<EventContainer, List<long>>(s, new List<long>())));
                            break;

                        case "OnMoveOnMap":
                            even.Item2.ToList().ForEach(s => evt.MapInstance.OnMoveOnMapEvents.Add(s));
                            break;

                        case "OnMapClean":
                            even.Item2.ToList().ForEach(s => evt.MapInstance.OnMapClean.Add(s));
                            break;

                        case "OnLockerOpen":
                            if (evt.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                            {
                                even.Item2.ToList().ForEach(s => evt.MapInstance.UnlockEvents.Add(s));
                                break;
                            }
                            even.Item2.ToList().ForEach(s => evt.MapInstance.InstanceBag.UnlockEvents.Add(s));
                            break;
                    }
                    break;
                case EventActionType.REGISTERWAVE:
                    evt.MapInstance.WaveEvents.Add((EventWave)evt.Parameter);
                    break;

                case EventActionType.SETAREAENTRY:
                    ZoneEvent even2 = (ZoneEvent)evt.Parameter;
                    evt.MapInstance.OnAreaEntryEvents.Add(even2);
                    break;

                case EventActionType.REMOVEMONSTERLOCKER:
                    session = evt.MapInstance.Sessions.FirstOrDefault();
                    if (evt.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        if (evt.MapInstance.MonsterLocker.Current > 0)
                        {
                            evt.MapInstance.MonsterLocker.Current--;
                        }
                        if (evt.MapInstance.MonsterLocker.Current == 0 && evt.MapInstance.ButtonLocker.Current == 0)
                        {
                            evt.MapInstance.UnlockEvents.ToList().ForEach(s => RunEvent(s));
                            evt.MapInstance.UnlockEvents.Clear();
                        }
                        evt.MapInstance.Broadcast(session?.Character?.Group?.GeneraterRaidmbf(evt.MapInstance));
                        break;
                    }
                    if (evt.MapInstance.InstanceBag.MonsterLocker.Current > 0)
                    {
                        evt.MapInstance.InstanceBag.MonsterLocker.Current--;
                    }
                    if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                    {
                        evt.MapInstance.InstanceBag.UnlockEvents.ToList().ForEach(s => RunEvent(s));
                        evt.MapInstance.InstanceBag.UnlockEvents.Clear();
                    }
                    break;

                case EventActionType.REMOVEBUTTONLOCKER:
                    session = evt.MapInstance.Sessions.FirstOrDefault();
                    if (evt.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        if (evt.MapInstance.ButtonLocker.Current > 0)
                        {
                            evt.MapInstance.ButtonLocker.Current--;
                        }
                        if (evt.MapInstance.MonsterLocker.Current == 0 && evt.MapInstance.ButtonLocker.Current == 0)
                        {
                            evt.MapInstance.UnlockEvents.ToList().ForEach(s => RunEvent(s));
                            evt.MapInstance.UnlockEvents.Clear();
                        }
                        evt.MapInstance.Broadcast(session?.Character?.Group?.GeneraterRaidmbf(evt.MapInstance));
                        break;
                    }
                    if (evt.MapInstance.InstanceBag.ButtonLocker.Current > 0)
                    {
                        evt.MapInstance.InstanceBag.ButtonLocker.Current--;
                    }
                    if (evt.MapInstance.InstanceBag.MonsterLocker.Current == 0 && evt.MapInstance.InstanceBag.ButtonLocker.Current == 0)
                    {
                        evt.MapInstance.InstanceBag.UnlockEvents.ToList().ForEach(s => RunEvent(s));
                        evt.MapInstance.InstanceBag.UnlockEvents.Clear();
                    }
                    break;

                case EventActionType.EFFECT:
                    short evt3 = (short)evt.Parameter;
                    if (monster != null && (DateTime.Now - monster.LastEffect).TotalSeconds >= 5)
                    {
                        evt.MapInstance.Broadcast(monster.GenerateEff(evt3));
                        monster.ShowEffect();
                    }
                    break;

                case EventActionType.INSTANTBATLLEREWARDS:
                    RunEvent(new EventContainer(evt.MapInstance, EventActionType.SPAWNPORTAL, new Portal { SourceX = 47, SourceY = 33, DestinationMapId = 1 }));
                    evt.MapInstance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("INSTANTBATTLE_SUCCEEDED"), 0));
                    Parallel.ForEach(evt.MapInstance.Sessions.Where(s => s.Character != null), cli =>
                    {
                        cli.Character.GetReput(cli.Character.Level * 50);
                        cli.Character.GetGold(cli.Character.Level * 1000);
                        cli.Character.SpAdditionPoint += cli.Character.Level * 100;
                        cli.Character.SpAdditionPoint = cli.Character.SpAdditionPoint > 1000000 ? 1000000 : cli.Character.SpAdditionPoint;
                        cli.SendPacket(cli.Character.GenerateSpPoint());
                        cli.SendPacket(cli.Character.GenerateGold());
                        cli.SendPacket(cli.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_SP_POINT"), cli.Character.Level * 100), 10));
                    });
                    break;
                case EventActionType.CONTROLEMONSTERINRANGE:
                    if (monster != null)
                    {
                        Tuple<short, byte, ConcurrentBag<EventContainer>> evnt = (Tuple<short, byte, ConcurrentBag<EventContainer>>)evt.Parameter;
                        List<MapMonster> mapMonsters = evt.MapInstance.GetListMonsterInRange(monster.MapX, monster.MapY, evnt.Item2);
                        if (evnt.Item1 != 0)
                        {
                            mapMonsters.RemoveAll(s => s.MonsterVNum != evnt.Item1);
                        }
                        mapMonsters.ForEach(s => evnt.Item3.ToList().ForEach(e => RunEvent(e, monster: s)));
                    }
                    break;

                case EventActionType.ONTARGET:
                    if (monster?.MoveEvent != null && monster.MoveEvent.InZone(monster.MapX, monster.MapY))
                    {
                        monster.MoveEvent = null;
                        monster.Path = null;
                        foreach (EventContainer s in ((ConcurrentBag<EventContainer>)evt.Parameter))
                        {
                            RunEvent(s, monster: monster);
                        }
                    }
                    break;

                case EventActionType.MOVE:
                    ZoneEvent evt4 = (ZoneEvent)evt.Parameter;
                    if (monster != null)
                    {
                        monster.MoveEvent = evt4;
                        monster.Path = BestFirstSearch.FindPath(new Node { X = monster.MapX, Y = monster.MapY }, new Node { X = evt4.X, Y = evt4.Y }, evt.MapInstance?.Map.Grid);
                    }
                    break;

                case EventActionType.CLOCK:
                    evt.MapInstance.InstanceBag.Clock.BasesSecondRemaining = Convert.ToInt32(evt.Parameter);
                    evt.MapInstance.InstanceBag.Clock.DeciSecondRemaining = Convert.ToInt32(evt.Parameter);
                    break;

                case EventActionType.SETMONSTERLOCKERS:
                    if (evt.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        evt.MapInstance.MonsterLocker.Current = Convert.ToByte(evt.Parameter);
                        evt.MapInstance.MonsterLocker.Initial = Convert.ToByte(evt.Parameter);
                        break;
                    }
                    evt.MapInstance.InstanceBag.MonsterLocker.Current = Convert.ToByte(evt.Parameter);
                    evt.MapInstance.InstanceBag.MonsterLocker.Initial = Convert.ToByte(evt.Parameter);
                    break;

                case EventActionType.SETBUTTONLOCKERS:
                    if (evt.MapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        evt.MapInstance.ButtonLocker.Current = Convert.ToByte(evt.Parameter);
                        evt.MapInstance.ButtonLocker.Initial = Convert.ToByte(evt.Parameter);
                        break;
                    }
                    evt.MapInstance.InstanceBag.ButtonLocker.Current = Convert.ToByte(evt.Parameter);
                    evt.MapInstance.InstanceBag.ButtonLocker.Initial = Convert.ToByte(evt.Parameter);
                    break;

                case EventActionType.SCRIPTEND:
                    ClientSession client = evt.MapInstance.Sessions.FirstOrDefault();
                    if (client == null)
                    {
                        return;
                    }
                    switch (evt.MapInstance.MapInstanceType)
                    {
                        case MapInstanceType.TimeSpaceInstance:
                            evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;
                            Guid mapInstanceId = ServerManager.Instance.GetBaseMapInstanceIdByMapId(client.Character.MapId);
                            MapInstance map = ServerManager.Instance.GetMapInstance(mapInstanceId);
                            ScriptedInstance si = map?.ScriptedInstances?.FirstOrDefault(s => s.PositionX == client.Character.MapX && s.PositionY == client.Character.MapY);
                            if (si == null)
                            {
                                return;
                            }

                            byte penalty = (byte)(client.Character.Level - si.LevelMinimum > 50 ? 100 : (client.Character.Level - si.LevelMinimum) * 2);
                            if (penalty > 0)
                            {
                                client.SendPacket(client.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("TS_PENALTY"), penalty), 10));
                            }
                            int point = evt.MapInstance.InstanceBag.Point * (100 - penalty) / 100;
                            string perfection = $"{(evt.MapInstance.InstanceBag.MonstersKilled >= si.MonsterAmount ? 1 : 0)}{(evt.MapInstance.InstanceBag.NpcsKilled == 0 ? 1 : 0)}{(evt.MapInstance.InstanceBag.RoomsVisited >= si.RoomAmount ? 1 : 0)}";
                            evt.MapInstance.Broadcast(
                                $"score  {evt.MapInstance.InstanceBag.EndState} {point} 27 47 18 {si.DrawItems.Count} {evt.MapInstance.InstanceBag.MonstersKilled} {si.NpcAmount - evt.MapInstance.InstanceBag.NpcsKilled} {evt.MapInstance.InstanceBag.RoomsVisited} {perfection} 1 1");
                            break;

                        case MapInstanceType.RaidInstance:
                            evt.MapInstance.InstanceBag.EndState = (byte)evt.Parameter;

                            if (client.Character?.Family?.Act4Raid?.Maps?.FirstOrDefault(m => m != client.Character?.Family?.Act4Raid?.FirstMap) == evt.MapInstance) // Act 4 raids
                            {
                                ScriptedInstance instance = client.Character?.Family?.Act4Raid;
                                if (instance?.FirstMap == null)
                                {
                                    return;
                                }

                                Instance.RunEvent(new EventContainer(instance.FirstMap, EventActionType.REMOVEPORTAL, instance.FirstMap.Portals.FirstOrDefault(port => port.DestinationMapInstanceId == client.CurrentMapInstance.MapInstanceId)));
                                Instance.ScheduleEvent(TimeSpan.FromSeconds(5), new EventContainer(evt.MapInstance, EventActionType.SENDPACKET, UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("TELEPORTED_IN"), 10), 0)));
                                foreach (ClientSession cli in evt.MapInstance.Sessions)
                                {
                                    if (evt.MapInstance.Sessions.Count(s => s.IpAddress.Equals(cli.IpAddress)) > 2 || instance.GiftItems == null)
                                    {
                                        continue;
                                    }
                                    foreach (Gift gift in instance.GiftItems)
                                    {
                                        sbyte rare = (sbyte)(gift.IsRandomRare ? ServerManager.Instance.RandomNumber(1, 8) : 0);
                                        if (cli.Character.Level >= instance.LevelMinimum)
                                        {
                                            cli.Character.GiftAdd(gift.VNum, gift.Amount, gift.Design, rare: rare);
                                        }
                                    }
                                    cli.Character.IncrementQuests(QuestType.WinRaid, instance.Id);
                                }

                                Observable.Timer(TimeSpan.FromSeconds(15)).Subscribe(s =>
                                {
                                    evt.MapInstance.Sessions.ToList().ForEach(cli =>
                                    ServerManager.Instance.ChangeMapInstance(cli.Character.CharacterId, instance.FirstMap.MapInstanceId, instance.StartX, instance.StartY));
                                });
                                break;
                            }

                            // Raids
                            Group grp = client.Character?.Group;
                            if (grp == null)
                            {
                                return;
                            }
                            if (evt.MapInstance.InstanceBag.EndState == 1 && evt.MapInstance.Monsters.Any(s => s.IsBoss && !s.IsAlive))
                            {
                                foreach (ClientSession sess in grp.Characters.Where(s => s.CurrentMapInstance.Monsters.Any(e => e.IsBoss)))
                                {
                                    // TODO REMOTE THAT FOR PUBLIC RELEASE
                                    if (grp.Characters.Count(s => s.IpAddress.Equals(sess.IpAddress)) > 2 || grp.Raid?.GiftItems == null)
                                    {
                                        continue;
                                    }
                                    if (grp.Raid.Reputation > 0 && sess.Character.Level > grp.Raid.LevelMinimum)
                                    {
                                        sess.Character.GetReput(grp.Raid.Reputation);
                                    }
                                    sess.Character.Dignity = sess.Character.Dignity < 0 ? sess.Character.Dignity + 100 : 100;

                                    if (sess.Character.Level > grp.Raid.LevelMaximum)
                                    {
                                        sess.Character.GiftAdd(2320, 1); // RAID CERTIFICATE
                                        continue;
                                    }
                                    foreach (Gift gift in grp.Raid?.GiftItems)
                                    {
                                        sbyte rare = (sbyte)(gift.IsRandomRare ? ServerManager.Instance.RandomNumber(-2, 8) : 0);

                                        if (sess.Character.Level >= grp.Raid.LevelMinimum)
                                        {
                                            sess.Character.GiftAdd(gift.VNum, gift.Amount, gift.Design, rare: rare);
                                        }
                                    }
                                }
                                // Remove monster when raid is over
                                evt.MapInstance.Monsters.Where(s => !s.IsBoss).ToList().ForEach(m => evt.MapInstance.DespawnMonster(m));
                                evt.MapInstance.WaveEvents.Clear();

                                ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("RAID_SUCCEED"), grp.Raid?.Label, grp.Characters.ElementAt(0).Character.Name), 0));
                            }
                            Observable.Timer(TimeSpan.FromSeconds(evt.MapInstance.InstanceBag.EndState == 1 ? 30 : 0)).Subscribe(obj =>
                            {
                                ClientSession[] grpmembers = new ClientSession[40];
                                grp.Characters.ToList().CopyTo(grpmembers);
                                foreach (ClientSession targetSession in grpmembers)
                                {
                                    if (targetSession == null)
                                    {
                                        continue;
                                    }
                                    if (targetSession.Character.Hp <= 0)
                                    {
                                        targetSession.Character.Hp = 1;
                                        targetSession.Character.Mp = 1;
                                    }
                                    targetSession.SendPacket(targetSession.Character.GenerateRaidBf(evt.MapInstance.InstanceBag.EndState));
                                    targetSession.SendPacket(targetSession.Character.GenerateRaid(1, true));
                                    targetSession.SendPacket(targetSession.Character.GenerateRaid(2, true));
                                    grp.LeaveGroup(targetSession);
                                }
                                ServerManager.Instance.GroupList.RemoveAll(s => s.GroupId == grp.GroupId);
                                ServerManager.Instance._groups.TryRemove(grp.GroupId, out Group _);
                                grp.Raid.Mapinstancedictionary.Values.ToList().ForEach(m => m.Dispose());
                            });
                            break;
                    }
                    break;

                case EventActionType.MAPCLOCK:
                    evt.MapInstance.Clock.BasesSecondRemaining = Convert.ToInt32(evt.Parameter);
                    evt.MapInstance.Clock.DeciSecondRemaining = Convert.ToInt32(evt.Parameter);
                    break;

                case EventActionType.STARTCLOCK:
                    Tuple<ConcurrentBag<EventContainer>, ConcurrentBag<EventContainer>> eve = (Tuple<ConcurrentBag<EventContainer>, ConcurrentBag<EventContainer>>)evt.Parameter;
                    if (eve != null)
                    {
                        evt.MapInstance.InstanceBag.Clock.StopEvents = eve.Item2.ToList();
                        evt.MapInstance.InstanceBag.Clock.TimeoutEvents = eve.Item1.ToList();
                        evt.MapInstance.InstanceBag.Clock.StartClock();
                        evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                    }
                    break;

                case EventActionType.TELEPORT:
                    Tuple<short, short, short, short> tp = (Tuple<short, short, short, short>)evt.Parameter;
                    List<Character> characters = evt.MapInstance.GetCharactersInRange(tp.Item1, tp.Item2, 5).ToList();
                    characters.ForEach(s =>
                    {
                        s.PositionX = tp.Item3;
                        s.PositionY = tp.Item4;
                        evt.MapInstance?.Broadcast(s.Session, s.GenerateTp(), ReceiverType.Group);
                    });
                    break;

                case EventActionType.STOPCLOCK:
                    evt.MapInstance.InstanceBag.Clock.StopClock();
                    evt.MapInstance.Broadcast(evt.MapInstance.InstanceBag.Clock.GetClock());
                    break;

                case EventActionType.STARTMAPCLOCK:
                    eve = (Tuple<ConcurrentBag<EventContainer>, ConcurrentBag<EventContainer>>)evt.Parameter;
                    if (eve != null)
                    {
                        evt.MapInstance.Clock.StopEvents = eve.Item2.ToList();
                        evt.MapInstance.Clock.TimeoutEvents = eve.Item1.ToList();
                        evt.MapInstance.Clock.StartClock();
                        evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                    }
                    break;

                case EventActionType.STOPMAPCLOCK:
                    evt.MapInstance.Clock.StopClock();
                    evt.MapInstance.Broadcast(evt.MapInstance.Clock.GetClock());
                    break;

                case EventActionType.SPAWNPORTAL:
                    evt.MapInstance.CreatePortal((Portal)evt.Parameter);
                    break;

                case EventActionType.REFRESHMAPITEMS:
                    evt.MapInstance.MapClear();
                    break;

                case EventActionType.NPCSEFFECTCHANGESTATE:
                    evt.MapInstance.Npcs.ForEach(s => s.EffectActivated = (bool)evt.Parameter);
                    break;

                case EventActionType.CHANGEPORTALTYPE:
                    Tuple<int, PortalType> param = (Tuple<int, PortalType>)evt.Parameter;
                    Portal portal = evt.MapInstance.Portals.FirstOrDefault(s => s.PortalId == param.Item1);
                    if (portal != null)
                    {
                        portal.Type = (short)param.Item2;
                    }
                    break;

                case EventActionType.CHANGEDROPRATE:
                    evt.MapInstance.DropRate = (int)evt.Parameter;
                    break;

                case EventActionType.CHANGEXPRATE:
                    evt.MapInstance.XpRate = (int)evt.Parameter;
                    break;

                case EventActionType.DISPOSEMAP:
                    evt.MapInstance.Dispose();
                    break;

                case EventActionType.SPAWNBUTTON:
                    evt.MapInstance.SpawnButton((MapButton)evt.Parameter);
                    break;

                case EventActionType.UNSPAWNMONSTERS:
                    evt.MapInstance.DespawnMonster((int)evt.Parameter);
                    break;

                case EventActionType.SPAWNMONSTERS:
                    evt.MapInstance.SummonMonsters(((ConcurrentBag<ToSummon>)evt.Parameter).ToList());
                    break;
                case EventActionType.REFRESHRAIDGOAL:
                    ClientSession cl = evt.MapInstance.Sessions.FirstOrDefault();
                    if (cl?.Character != null)
                    {
                        evt.MapInstance.Broadcast(cl.Character?.Group?.GeneraterRaidmbf(evt.MapInstance));
                        ServerManager.Instance.Broadcast(cl, UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NEW_MISSION"), 0), ReceiverType.Group);
                    }
                    break;
                case EventActionType.SPAWNNPCS:
                    evt.MapInstance.SummonNpcs((List<ToSummon>)evt.Parameter);
                    break;

                case EventActionType.DROPITEMS:
                    evt.MapInstance.DropItems((List<Tuple<short, int, short, short>>)evt.Parameter);
                    break;

                case EventActionType.THROWITEMS:
                    Tuple<int, short, byte, int, int> parameters = (Tuple<int, short, byte, int, int>)evt.Parameter;
                    if (monster != null)
                    {
                        parameters = new Tuple<int, short, byte, int, int>(monster.MapMonsterId, parameters.Item2, parameters.Item3, parameters.Item4, parameters.Item5);
                    }
                    evt.MapInstance.ThrowItems(parameters);
                    break;

                case EventActionType.SPAWNONLASTENTRY:
                    Character lastincharacter = evt.MapInstance.Sessions.OrderByDescending(s => s.RegisterTime).FirstOrDefault()?.Character;
                    List<ToSummon> summonParameters = new List<ToSummon>();
                    MapCell hornSpawn = new MapCell
                    {
                        X = lastincharacter?.PositionX ?? 154,
                        Y = lastincharacter?.PositionY ?? 140
                    };
                    summonParameters.Add(new ToSummon(Convert.ToInt16(evt.Parameter), hornSpawn, lastincharacter, true));
                    evt.MapInstance.SummonMonsters(summonParameters);
                    break;

                case EventActionType.REMOVEPORTAL:
                    Portal portalToRemove = evt.Parameter is Portal p ? evt.MapInstance.Portals.FirstOrDefault(s => s == p) : evt.MapInstance.Portals.FirstOrDefault(s => s.PortalId == (int)evt.Parameter);
                    if (portalToRemove == null)
                    {
                        return;
                    }
                    evt.MapInstance.Portals.Remove(portalToRemove);
                    evt.MapInstance.MapClear();
                    break;

                case EventActionType.ACT4RAIDEND:
                    // tp // tp X // tp Y
                    Tuple<MapInstance, short, short> endParameters = (Tuple<MapInstance, short, short>)evt.Parameter;
                    Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(a =>
                    {
                        evt.MapInstance.Broadcast($"{UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("TELEPORTED_IN"), 10), 0)}");

                        Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(s =>
                        {
                            evt.MapInstance.Sessions.ToList().ForEach(cli =>
                                ServerManager.Instance.ChangeMapInstance(cli.Character.CharacterId, endParameters.Item1.MapInstanceId, endParameters.Item2, endParameters.Item3));
                        });
                    });
                    break;

                case EventActionType.CLEARMAPMONSTERS:
                    evt.MapInstance.Monsters.ForEach(m => evt.MapInstance.DespawnMonster(m));
                    break;

                case EventActionType.STARTACT4RAID:
                    Tuple<byte, byte> raidParameters = (Tuple<byte, byte>)evt.Parameter;
                    Act4Stat stat = raidParameters.Item2 == (byte) FactionType.Angel ? ServerManager.Instance.Act4AngelStat : ServerManager.Instance.Act4DemonStat;
                    stat.Mode = 3;
                    stat.TotalTime = 3600;
                    Act4Raid.Instance.GenerateRaid(raidParameters.Item1, raidParameters.Item2);
                    switch ((Act4RaidType)raidParameters.Item1)
                    {
                        case Act4RaidType.Morcos:
                            stat.IsMorcos = true;
                            break;
                        case Act4RaidType.Hatus:
                            stat.IsHatus = true;
                            break;
                        case Act4RaidType.Calvina:
                            stat.IsCalvina = true;
                            break;
                        case Act4RaidType.Berios:
                            stat.IsBerios = true;
                            break;
                    }
                    break;

                case EventActionType.ONTIMEELAPSED:
                    Tuple<int, ConcurrentBag<EventContainer>> timeElapsedEvts = (Tuple<int, ConcurrentBag<EventContainer>>)evt.Parameter;
                    Observable.Timer(TimeSpan.FromSeconds(timeElapsedEvts.Item1)).Subscribe(e => {
                        timeElapsedEvts.Item2.ToList().ForEach(ev => RunEvent(ev));
                    });
                    break;


                #endregion
            }
        }

        public void ScheduleEvent(TimeSpan timeSpan, EventContainer evt)
        {
            Observable.Timer(timeSpan).Subscribe(x => { RunEvent(evt); });
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