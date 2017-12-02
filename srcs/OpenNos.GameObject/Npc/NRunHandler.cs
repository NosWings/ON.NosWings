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
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System.Collections.Concurrent;

namespace OpenNos.GameObject
{
    public static class NRunHandler
    {
        #region Methods

        public static void NRun(ClientSession session, NRunPacket packet)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }
            MapNpc npc = session.CurrentMapInstance.Npcs.FirstOrDefault(s => s.MapNpcId == packet.NpcId);
            TeleporterDTO tp;
            Random rand = new Random();
            switch (packet.Runner)
            {
                case 1:
                    if (session.Character.Class != (byte)ClassType.Adventurer)
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                        return;
                    }
                    if (session.Character.Level < 15 || session.Character.JobLevel < 20)
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                        return;
                    }
                    if (packet.Type == (byte)session.Character.Class)
                    {
                        return;
                    }
                    if (session.Character.Inventory.All(i => i.Value.Type != InventoryType.Wear))
                    {
                        session.Character.Inventory.AddNewToInventory((short)(4 + packet.Type * 14), type: InventoryType.Wear);
                        session.Character.Inventory.AddNewToInventory((short)(81 + packet.Type * 13), type: InventoryType.Wear);
                        switch (packet.Type)
                        {
                            case 1:
                                session.Character.Inventory.AddNewToInventory(68, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2082, 10);
                                break;

                            case 2:
                                session.Character.Inventory.AddNewToInventory(78, type: InventoryType.Wear);
                                session.Character.Inventory.AddNewToInventory(2083, 10);
                                break;

                            case 3:
                                session.Character.Inventory.AddNewToInventory(86, type: InventoryType.Wear);
                                break;
                        }
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateEq());
                        session.SendPacket(session.Character.GenerateEquipment());
                        session.Character.ChangeClass((ClassType)packet.Type);
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                case 2:
                    session.SendPacket("wopen 1 0");
                    break;

                case 4:
                    Mate mate = session.Character.Mates.FirstOrDefault(s => s.MateTransportId == packet.NpcId);
                    switch (packet.Type)
                    {
                        case 2:
                            if (mate != null)
                            {
                                if (session.Character.Level >= mate.Level)
                                {
                                    Mate teammate = session.Character.Mates.Where(s => s.IsTeamMember).FirstOrDefault(s => s.MateType == mate.MateType);
                                    if (teammate != null)
                                    {
                                        // TODO NEED TO FIND A WAY TO APPLY BUFFS PROPERLY THROUGH MONSTER SKILLS
                                        MateHelper.Instance.RemovePetBuffs(session);
                                        MateHelper.Instance.AddPetBuff(session);
                                        teammate.IsTeamMember = false;
                                        teammate.MapX = teammate.PositionX;
                                        teammate.MapY = teammate.PositionY;
                                    }
                                    mate.IsTeamMember = true;
                                }
                                else
                                {
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                                }
                            }
                            break;

                        case 3:
                            if (mate != null && session.Character.Miniland == session.Character.MapInstance)
                            {
                                // TODO NEED TO FIND A WAY TO APPLY BUFFS PROPERLY THROUGH MONSTER SKILLS
                                MateHelper.Instance.RemovePetBuffs(session);
                                mate.IsTeamMember = false;
                                mate.MapX = mate.PositionX;
                                mate.MapY = mate.PositionY;
                            }
                            break;

                        case 4:
                            if (mate != null)
                            {
                                if (session.Character.Miniland == session.Character.MapInstance)
                                {
                                    // TODO NEED TO FIND A WAY TO APPLY BUFFS PROPERLY THROUGH MONSTER SKILLS
                                    MateHelper.Instance.RemovePetBuffs(session);
                                    mate.IsTeamMember = false;
                                    mate.MapX = mate.PositionX;
                                    mate.MapY = mate.PositionY;
                                }
                                else
                                {
                                    session.SendPacket($"qna #n_run^4^5^3^{mate.MateTransportId} {Language.Instance.GetMessageFromKey("ASK_KICK_PET")}");
                                }
                                break;
                            }
                            break;

                        case 5:
                            if (mate != null)
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 10, $"#n_run^4^6^3^{mate.MateTransportId}"));
                            }
                            break;

                        case 6:
                            if (mate != null)
                            {
                                if (session.Character.Miniland != session.Character.MapInstance)
                                {
                                    // TODO NEED TO FIND A WAY TO APPLY BUFFS PROPERLY THROUGH MONSTER SKILLS
                                    MateHelper.Instance.RemovePetBuffs(session);
                                    mate.IsTeamMember = false;
                                    session.CurrentMapInstance.Broadcast(mate.GenerateOut());
                                    session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name), 11));
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("PET_KICKED"), mate.Name), 0));
                                }
                            }
                            break;

                        case 7:
                            if (mate != null)
                            {
                                if (session.Character.Mates.Any(s => s.MateType == mate.MateType && s.IsTeamMember))
                                {
                                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 11));
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("ALREADY_PET_IN_TEAM"), 0));
                                }
                                else
                                {
                                    MateHelper.Instance.RemovePetBuffs(session);
                                    session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 10, $"#n_run^4^9^3^{mate.MateTransportId}"));
                                }
                            }
                            break;

                        case 9:
                            if (mate != null)
                            {
                                if (session.Character.Level >= mate.Level)
                                {
                                    mate.PositionX = (short)(session.Character.PositionX + 1);
                                }
                                mate.PositionY = (short)(session.Character.PositionY + 1);
                                mate.IsTeamMember = true;
                                session.CurrentMapInstance.Broadcast(mate.GenerateIn());
                            }
                            else
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("PET_HIGHER_LEVEL"), 0));
                            }

                            break;
                    }
                    session.SendPacket(session.Character.GeneratePinit());
                    session.SendPackets(session.Character.Mates.Where(s => s.IsTeamMember)
                        .OrderBy(s => s.MateType)
                        .Select(s => s.GeneratePst()));
                    break;

                case 10:
                    session.SendPacket("wopen 3 0");
                    break;

                case 12:
                    session.SendPacket($"wopen {packet.Type} 0");
                    break;

                case 14:
                    session.SendPacket("wopen 27 0");
                    string recipelist = "m_list 2";
                    if (npc != null)
                    {
                        List<Recipe> tps = npc.Recipes;
                        recipelist = tps.Where(s => s.Amount > 0).Aggregate(recipelist, (current, s) => current + $" {s.ItemVNum}");
                        recipelist += " -100";
                        session.SendPacket(recipelist);
                    }
                    break;

                case 15:
                    if (npc != null)
                    {
                        if (packet.Value == 2)
                        {
                            session.SendPacket($"qna #n_run^15^1^1^{npc.MapNpcId} {Language.Instance.GetMessageFromKey("ASK_CHANGE_SPAWNLOCATION")}");
                        }
                        else
                        {
                            switch (npc.MapId)
                            {
                                case 1:
                                    session.Character.SetRespawnPoint(1, 79, 116);
                                    break;

                                case 20:
                                    session.Character.SetRespawnPoint(20, 9, 92);
                                    break;

                                case 145:
                                    session.Character.SetRespawnPoint(145, 13, 110);
                                    break;
                            }
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("RESPAWNLOCATION_CHANGED"), 0));
                        }
                    }
                    break;

                case 16:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 1000 * packet.Type)
                        {
                            session.Character.Gold -= 1000 * packet.Type;
                            session.SendPacket(session.Character.GenerateGold());
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"),
                                    10));
                        }
                    }
                    break;

                case 17:
                    double currentRunningSeconds = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                    double timeSpanSinceLastPortal = currentRunningSeconds - session.Character.LastPortal;
                    if (!(timeSpanSinceLastPortal >= 4) || !session.HasCurrentMapInstance || session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4Instance)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_MOVE"), 10));
                        return;
                    }
                    if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_JOIN_ARENA_IN_RAID"), 10));
                        return;
                    }
                    if (session.Character.Gold >= 500 * (1 + packet.Type))
                    {
                        session.Character.LastPortal = currentRunningSeconds;
                        session.Character.Gold -= 500 * (1 + packet.Type);
                        session.SendPacket(session.Character.GenerateGold());
                        ServerManager.Instance.TeleportOnRandomPlaceInMap(session,
                            packet.Type == 0 ? ServerManager.Instance.ArenaInstance.MapInstanceId : ServerManager.Instance.FamilyArenaInstance.MapInstanceId);
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                    }
                    break;

                case 18:
                    session.SendPacket(session.Character.GenerateNpcDialog(17));
                    break;

                case 26:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 5000 * packet.Type)
                        {
                            session.Character.Gold -= 5000 * packet.Type;
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                        }
                    }
                    break;

                case 45:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        if (session.Character.Gold >= 500)
                        {
                            session.Character.Gold -= 500;
                            session.SendPacket(session.Character.GenerateGold());
                            ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                        }
                        else
                        {
                            session.SendPacket(
                                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"),
                                    10));
                        }
                    }
                    break;

                case 132:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }
                    break;

                case 137:
                    session.SendPacket("taw_open");
                    break;

                case 138:
                    ConcurrentBag<ArenaTeamMember> at = ServerManager.Instance.ArenaTeams.OrderBy(s => rand.Next()).FirstOrDefault();
                    if (at != null)
                    {
                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, at.FirstOrDefault(s => s.Session != null).Session.CurrentMapInstance.MapInstanceId, 69, 100);

                        ArenaTeamMember zenas = at.OrderBy(s => s.Order).FirstOrDefault(s => s.Session != null && !s.Dead && s.ArenaTeamType == ArenaTeamType.ZENAS);
                        ArenaTeamMember erenia = at.OrderBy(s => s.Order).FirstOrDefault(s => s.Session != null && !s.Dead && s.ArenaTeamType == ArenaTeamType.ERENIA);
                        session.SendPacket(erenia.Session.Character.GenerateTaM(0));
                        session.SendPacket(erenia.Session.Character.GenerateTaM(3));
                        session.SendPacket("taw_sv 0");
                        session.SendPacket(zenas.Session.Character.GenerateTaP(0, true));
                        session.SendPacket(erenia.Session.Character.GenerateTaP(2, true));
                        session.SendPacket(zenas.Session.Character.GenerateTaFc(0));
                        session.SendPacket(erenia.Session.Character.GenerateTaFc(1));
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NO_TALENT_ARENA")));
                    }
                    break;
                case 135:
                    if (!ServerManager.Instance.StartedEvents.Contains(EventType.TALENTARENA))
                    {
                        session.SendPacket(npc?.GenerateSay(Language.Instance.GetMessageFromKey("ARENA_NOT_OPEN"), 10));
                    }
                    else
                    {
                        int tickets = 5 - session.Character.GeneralLogs.Count(s => s.LogType == "TalentArena" && s.Timestamp.Date == DateTime.Today);
                        if (ServerManager.Instance.ArenaMembers.All(s => s.Session != session) && tickets > 0)
                        {
                            if (ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId))
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("TALENT_ARENA_GROUP"), 0));
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TALENT_ARENA_GROUP"), 10));
                            }
                            else
                            {
                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ARENA_TICKET_LEFT"), tickets), 10));
                                ServerManager.Instance.ArenaMembers.Add(new ArenaMember
                                {
                                    ArenaType = EventType.TALENTARENA,
                                    Session = session,
                                    GroupId = null,
                                    Time = 0
                                });
                            }
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("TALENT_ARENA_NO_MORE_TICKET"), 0));
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TALENT_ARENA_NO_MORE_TICKET"), 10));
                        }
                    }
                    break;

                case 150:
                    if (npc != null)
                    {
                        if (session.Character.Family != null)
                        {
                            if (session.Character.Family.LandOfDeath == null)
                            {
                                session.Character.Family.LandOfDeath = ServerManager.Instance.GenerateMapInstance(150, MapInstanceType.LodInstance, new InstanceBag());
                            }
                            ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, session.Character.Family.LandOfDeath.MapInstanceId, 153, 145);
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_FAMILY"), 0));
                        }
                    }
                    break;

                case 301:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }
                    break;

                case 1600:
                    session.SendPacket(session.Character.OpenFamilyWarehouse());
                    break;

                case 1601:
                    session.SendPackets(session.Character.OpenFamilyWarehouseHist());
                    break;

                case 1602:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 3 && session.Character.Family.WarehouseSize < 21)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (500000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.WarehouseSize = 21;
                            session.Character.Gold -= 500000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1603:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 7 && session.Character.Family.WarehouseSize < 49)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (2000000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.WarehouseSize = 49;
                            session.Character.Gold -= 2000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1604:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 5 && session.Character.Family.MaxSize < 70)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (5000000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.MaxSize = 70;
                            session.Character.Gold -= 5000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 1605:
                    if (session.Character.Family != null && session.Character.Family.FamilyLevel >= 9 && session.Character.Family.MaxSize < 100)
                    {
                        if (session.Character.FamilyCharacter.Authority == FamilyAuthority.Head)
                        {
                            if (10000000 >= session.Character.Gold)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                                return;
                            }
                            session.Character.Family.MaxSize = 100;
                            session.Character.Gold -= 10000000;
                            session.SendPacket(session.Character.GenerateGold());
                            FamilyDTO fam = session.Character.Family;
                            DaoFactory.FamilyDao.InsertOrUpdate(ref fam);
                            ServerManager.Instance.FamilyRefresh(session.Character.Family.FamilyId);
                        }
                        else
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 10));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateModal(Language.Instance.GetMessageFromKey("ONLY_HEAD_CAN_BUY"), 1));
                        }
                    }
                    break;

                case 23:
                    if (packet.Type == 0)
                    {
                        if (session.Character.Group != null && session.Character.Group.CharacterCount == 3)
                        {
                            if (session.Character.Group.Characters.Any(s => s.Character.Family != null))
                            {
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("GROUP_MEMBER_ALREADY_IN_FAMILY")));
                                return;
                            }
                        }
                        if (session.Character.Group == null || session.Character.Group.CharacterCount != 3)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("FAMILY_GROUP_NOT_FULL")));
                            return;
                        }
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInbox($"#glmk^ {14} 1 {Language.Instance.GetMessageFromKey("CREATE_FAMILY").Replace(' ', '^')}"));
                    }
                    else
                    {
                        if (session.Character.Family == null)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_IN_FAMILY")));
                            return;
                        }
                        if (session.Character.Family != null && session.Character.FamilyCharacter != null && session.Character.FamilyCharacter.Authority != FamilyAuthority.Head)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_FAMILY_HEAD")));
                            return;
                        }
                        session.SendPacket($"qna #glrm^1 {Language.Instance.GetMessageFromKey("DISMISS_FAMILY")}");
                    }

                    break;

                case 60:
                    StaticBonusDTO medal = session.Character.StaticBonusList.FirstOrDefault(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver);
                    byte Medal = 0;
                    int Time = 0;
                    if (medal != null)
                    {
                        Medal = medal.StaticBonusType == StaticBonusType.BazaarMedalGold ? (byte)MedalType.Gold : (byte)MedalType.Silver;
                        Time = (int)(medal.DateEnd - DateTime.Now).TotalHours;
                    }
                    session.SendPacket($"wopen 32 {Medal} {Time}");
                    break;

                case 3000:
                    if (npc != null)
                    {
                        if (packet.Type == 0 && packet.Value == 2)
                        {
                            if (session.Character.Group?.Raid == null)
                            {
                                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("YOU_HAVE_TO_BE_IN_RAID"), 10));
                                return;
                            }
                            switch ((RaidDesignType)session.Character.Group?.Raid.Id)
                            {
                                case RaidDesignType.Cuby:
                                    break;

                                default:
                                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_QUEST_FOR_THIS_RAID"), 10));
                                    break;
                            }
                        }
                    }
                    break;

                case 5001:
                    if (npc != null)
                    {
                        switch (session.Character.Faction)
                        {
                            case FactionType.Neutral:
                                session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo("NEED_FACTION_ACT4"));
                                return;
                        }
                        if (3000 > session.Character.Gold)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                            return;
                        }
                        session.Character.Gold -= 3000;
                        session.Character.ConnectAct4();
                    }
                    break;

                case 5002:
                    if (npc != null)
                    {
                        tp = npc.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                        if (tp != null)
                        {
                            session.SendPacket("it 3");
                            SerializableWorldServer connection = CommunicationServiceClient.Instance.GetPreviousChannelByAccountId(session.Account.AccountId);
                            if (connection == null || session.Character == null)
                            {
                                break;
                            }
                            session.Character.MapId = tp.MapId;
                            session.Character.MapX = tp.MapX;
                            session.Character.MapY = tp.MapY;
                            session.Character.ChangeChannel(connection.EndPointIp, connection.EndPointPort, 3);
                        }
                    }
                    break;

                case 5011:
                    if (npc != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, 170, 127, 46);
                    }
                    break;

                case 5012:
                    tp = npc?.Teleporters?.FirstOrDefault(s => s.Index == packet.Type);
                    if (tp != null)
                    {
                        ServerManager.Instance.ChangeMap(session.Character.CharacterId, tp.MapId, tp.MapX, tp.MapY);
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_NRUN_HANDLER"), packet.Runner));
                    break;
            }
        }

        #endregion
    }
}