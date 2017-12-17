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
using CloneExtensions;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.GameObject.Item
{
    public class SpecialItem : Item
    {
        #region Instantiation

        public SpecialItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0, string[] packetsplit = null)
        {
            inv.Item.BCards.ForEach(c => c.ApplyBCards(session.Character));

            switch (Effect)
            {
                case 0:
                    switch (VNum)
                    {
                        case 5370:
                            if (session.Character.Buff.Any(s => s.Card.CardId == 393))
                            {
                                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ALREADY_GOT_BUFF"), session.Character.Buff.FirstOrDefault(s => s.Card.CardId == 393)?.Card.Name), 10));
                                return;
                            }
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                            session.Character.AddStaticBuff(new StaticBuffDTO { CardId = 393 });
                            break;
                        case 1428:
                            session.SendPacket("guri 18 1");
                            break;
                        case 1429:
                            session.SendPacket("guri 18 0");
                            break;
                        case 1430:
                            if (packetsplit == null)
                            {
                                return;
                            }

                            if (packetsplit.Length < 9)
                            {
                                // MODIFIED PACKET
                                return;
                            }

                            if (!short.TryParse(packetsplit[9], out short eqSlot) || !Enum.TryParse(packetsplit[8], out InventoryType eqType))
                            {
                                return;
                            }
                            WearableInstance eq = session.Character.Inventory.LoadBySlotAndType<WearableInstance>(eqSlot, eqType);
                            if (eq == null)
                            {
                                // PACKET MODIFIED
                                return;
                            }
                            if (eq.Item.ItemType != ItemType.Armor && eq.Item.ItemType != ItemType.Weapon)
                            {
                                return;
                            }
                            eq.EquipmentOptions.Clear();
                            eq.ShellRarity = null;
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SHELL_ERASED"), 0));
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(17, 1, session.Character.CharacterId));
                            break;
                        case 1904:
                            short[] items = { 1894, 1895, 1896, 1897, 1898, 1899, 1900, 1901, 1902, 1903 };
                            for (int i = 0; i < 5; i++)
                            {
                                session.Character.GiftAdd(items[ServerManager.Instance.RandomNumber(0, items.Length)], 1);
                            }
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                            break;
                        case 5916:
                            session.Character.AddStaticBuff(new StaticBuffDTO
                            {
                                CardId = 340,
                                CharacterId = session.Character.CharacterId,
                                RemainingTime = 7200
                            });
                            session.Character.RemoveBuff(339, true);
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                            break;
                        case 5929:
                            session.Character.AddStaticBuff(new StaticBuffDTO
                            {
                                CardId = 340,
                                CharacterId = session.Character.CharacterId,
                                RemainingTime = 600
                            });
                            session.Character.RemoveBuff(339, true);
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                            break;
                        default:
                            IEnumerable<RollGeneratedItemDTO> roll = DaoFactory.RollGeneratedItemDao.LoadByItemVNum(VNum);
                            IEnumerable<RollGeneratedItemDTO> rollGeneratedItemDtos = roll as IList<RollGeneratedItemDTO> ?? roll.ToList();
                            if (!rollGeneratedItemDtos.Any())
                            {
                                return;
                            }
                            int probabilities = rollGeneratedItemDtos.Sum(s => s.Probability);
                            int rnd = ServerManager.Instance.RandomNumber(0, probabilities);
                            int currentrnd = 0;
                            foreach (RollGeneratedItemDTO rollitem in rollGeneratedItemDtos)
                            {
                                if (rollitem.Probability == 10000)
                                {
                                    session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount);
                                    continue;
                                }
                                currentrnd += rollitem.Probability;
                                if (currentrnd < rnd)
                                {
                                    continue;
                                }
                                if (rollitem.IsSuperReward)
                                {
                                    CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                                    {
                                        DestinationCharacterId = null,
                                        SourceCharacterId = session.Character.CharacterId,
                                        SourceWorldId = ServerManager.Instance.WorldId,
                                        Message = Language.Instance.GetMessageFromKey("SUPER_REWARD"),
                                        Type = MessageType.Shout
                                    });
                                }
                                session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount, 0, rollitem.ItemGeneratedUpgrade);
                                break;
                            }
                            session.Character.Inventory.RemoveItemAmount(VNum);
                            break;
                    }
                    break;

                // sp point potions
                case 150:
                case 151:
                    session.Character.SpAdditionPoint += EffectValue;
                    if (session.Character.SpAdditionPoint > 1000000)
                    {
                        session.Character.SpAdditionPoint = 1000000;
                    }
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDED"), EffectValue), 0));
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateSpPoint());
                    break;

                case 204:
                    session.Character.SpPoint += EffectValue;
                    session.Character.SpAdditionPoint += EffectValue * 3;
                    if (session.Character.SpAdditionPoint > 1000000)
                    {
                        session.Character.SpAdditionPoint = 1000000;
                    }
                    if (session.Character.SpPoint > 10000)
                    {
                        session.Character.SpPoint = 10000;
                    }
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("SP_POINTSADDEDBOTH"), EffectValue, EffectValue * 3), 0));
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateSpPoint());
                    break;

                case 250:
                    if (session.Character.Buff.Any(s => s.Card.CardId == 131))
                    {
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ALREADY_GOT_BUFF"), session.Character.Buff.FirstOrDefault(s => s.Card.CardId == 131)?.Card.Name), 10));
                        return;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.Character.AddStaticBuff(new StaticBuffDTO { CardId = 131 });
                    session.CurrentMapInstance.Broadcast(session.Character.GeneratePairy());
                    session.SendPacket(session.Character.GeneratePairy());
                    break;

                case 208:
                    if (session.Character.Buff.Any(s => s.Card.CardId == 121))
                    {
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("ALREADY_GOT_BUFF"), session.Character.Buff.FirstOrDefault(s => s.Card.CardId == 121)?.Card.Name), 10));
                        return;
                    }
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.Character.AddStaticBuff(new StaticBuffDTO { CardId = 121, CharacterId = session.Character.CharacterId, RemainingTime = 3600 });
                    break;

                case 301:
                    if (ServerManager.Instance.IsCharacterMemberOfGroup(session.Character.CharacterId))
                    {
                        //TODO you are in group
                        return;
                    }
                    ItemInstance raidSeal = session.Character.Inventory.LoadBySlotAndType<ItemInstance>(inv.Slot, InventoryType.Main);

                    ScriptedInstance raid = ServerManager.Instance?.Raids?.FirstOrDefault(s =>
                        {
                            return s?.RequieredItems != null && s.RequieredItems.Any(obj => obj?.VNum == raidSeal?.ItemVNum);
                        })?.GetClone();
                    if (raid != null)
                    {
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, raidSeal.Id);
                        GroupType groupType = GroupType.Team;
                        switch (raidSeal.Item.EffectValue)
                        {
                            case 20:
                                groupType = GroupType.GiantTeam;
                                break;
                            case 23:
                                groupType = GroupType.BigTeam;
                                break;
                        }

                        Group group = new Group(groupType);
                        group.Raid = raid;
                        group.JoinGroup(session.Character.CharacterId);
                        ServerManager.Instance.AddGroup(group);
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("YOU_ARE_RAID_CHIEF"), session.Character.Name), 0));
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("YOU_ARE_RAID_CHIEF"), session.Character.Name), 10));
                        if (session.Character.Level > raid.LevelMaximum || session.Character.Level < raid.LevelMinimum)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RAID_LEVEL_INCORRECT"), 10));
                        }
                        session.SendPacket(session.Character.GenerateRaid(2, false));
                        session.SendPacket(session.Character.GenerateRaid(0, false));
                        session.SendPacket(session.Character.GenerateRaid(1, false));
                        session.SendPacket(group.GenerateRdlst());
                    }
                    break;

                case 305:
                    Mate partner = session.Character.Mates
                            .FirstOrDefault(x => x.IsTeamMember && (x.NpcMonsterVNum == 317 || x.NpcMonsterVNum == 318 || x.NpcMonsterVNum == 319) && x.MateType == MateType.Partner);
                    if (partner == null)
                    {
                        return;
                    }
                    switch (partner.NpcMonsterVNum)
                    {
                        case 317:
                            if (inv.Item.VNum == 1103 && partner.Skin != inv.Item.Morph)
                            {
                                partner.Skin = inv.Item.Morph;
                            }
                            else
                            {
                                return;
                            }
                            break;
                        case 318:
                            if (inv.Item.VNum == 1141 && partner.Skin != inv.Item.Morph)
                            {
                                partner.Skin = inv.Item.Morph;
                            }
                            else
                            {
                                return;
                            }
                            break;
                        case 319:
                            if (inv.Item.VNum == 1142 && partner.Skin != inv.Item.Morph)
                            {
                                partner.Skin = inv.Item.Morph;
                            }
                            else
                            {
                                return;
                            }
                            break;
                    }
                    session.Character?.Inventory?.RemoveItemAmountFromInventory(1, inv.Id);
                    session.CurrentMapInstance?.Broadcast(partner.GenerateCMode(partner.Skin));
                    break;

                //suction Funnel (Quest Item / QuestId = 1724)
                case 400:
                    if (session.Character == null || session.Character.Quests.All(q => q.QuestId != 1724))
                    {
                        break;
                    }
                    MapMonster kenko = session.CurrentMapInstance?.Monsters.FirstOrDefault(m => m.MapMonsterId == session.Character.LastMonsterId && m.MonsterVNum == 146);
                    if (kenko == null)
                    {
                        break;
                    }
                    kenko.KillMonster();
                    session.Character.Inventory.AddNewToInventory(1174); // Kenko Bead
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;

                //speed booster
                case 998:
                    if (!session.Character.IsVehicled || session.Character.Buff.Any(s => s.Card.CardId == 336))
                    {
                        return;
                    }
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(885), session.Character.MapX, session.Character.MapY);
                    session.Character.AddBuff(new Buff.Buff(336));
                    session.Character.Speed += 5;
                    switch (session.Character.Morph)
                    {
                        case 2517: // Nossi M
                        case 2518: // Nossi F
                        case 2522: // Roller M
                        case 2523: // Roller F
                                   // Removes <= lv 4 debuffs
                            List<BuffType> bufftodisable = new List<BuffType> { BuffType.Bad };
                            session.Character.DisableBuffs(bufftodisable, 4);
                            break;
                    }
                    Observable.Timer(TimeSpan.FromSeconds(session.Character.BuffRandomTime * 0.1D)).Subscribe(o =>
                    {
                        session.Character.Speed -= 5;
                        session.Character.LoadSpeed();
                        switch (session.Character.Morph)
                        {
                            case 2526: // White male unicorn
                            case 2527: // White female unicorn
                            case 2528: // Pink male unicorn
                            case 2529: // Pink female unicorn
                            case 2530: // Black male unicorn
                            case 2531: // Black Female Unicorn
                            case 2928: // Male UFO
                            case 2929: // Female UFO
                            case 3679: // Male squelettic dragon
                            case 3680: // Female squelettic dragon
                                ServerManager.Instance.TeleportOnRandomPlaceInMap(session, session.Character.MapInstanceId, true);
                                break;

                            case 2432: // Magic broom
                            case 2433: // Magic broom F
                            case 2520: // VTT M
                            case 2521: // VTT F
                                switch (session.Character.Direction)
                                {
                                    case 0:
                                        // -y
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, session.Character.PositionX, (short)(session.Character.PositionY - 5));
                                        break;
                                    case 1:
                                        // +x
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, (short)(session.Character.PositionX + 5), session.Character.PositionY);
                                        break;
                                    case 2:
                                        // +y
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, session.Character.PositionX, (short)(session.Character.PositionY + 5));
                                        break;
                                    case 3:
                                        // -x
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, (short)(session.Character.PositionX - 5), session.Character.PositionY);
                                        break;
                                    case 4:
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, (short)(session.Character.PositionX - 5), (short)(session.Character.PositionY - 5));
                                        // -x -y
                                        break;
                                    case 5:
                                        // +x +y
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, (short)(session.Character.PositionX - 5), (short)(session.Character.PositionY - 5));
                                        break;
                                    case 6:
                                        // +x -y
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, (short)(session.Character.PositionX + 5), (short)(session.Character.PositionY + 5));
                                        break;
                                    case 7:
                                        // -x +y
                                        ServerManager.Instance.TeleportForward(session, session.Character.MapInstanceId, (short)(session.Character.PositionX - 5), (short)(session.Character.PositionY + 5));
                                        break;
                                }
                                break;

                            case 2524:
                            case 2525:
                                if (session.Character.Hp > 0)
                                {
                                    session.Character.Hp += session.Character.Level * 15;
                                    if (session.Character.Hp > session.Character.HpLoad())
                                    {
                                        session.Character.Hp = (int)session.Character.HpLoad();
                                    }
                                }
                                break;
                        }
                    });
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;

                //Atk/Def/HP/Exp potions
                case 6600:
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;
                // Divorce letter
                case 6969: // this is imaginary number I = √(-1)
                    CharacterRelationDTO rel = session.Character.CharacterRelations.FirstOrDefault(s => s.RelationType == CharacterRelationType.Spouse);
                    if (rel != null)
                    {
                        session.Character.DeleteRelation(rel.CharacterId == session.Character.CharacterId ? rel.RelatedCharacterId : rel.CharacterId);
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(Language.Instance.GetMessageFromKey("DIVORCED")));
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    }
                    break;

                // Cupid's arrow
                case 34:
                    if (packetsplit != null && packetsplit.Length > 3)
                    {
                        if (long.TryParse(packetsplit[3], out long characterId))
                        {
                            if (session.Character.CharacterRelations.Any(s => s.RelationType == CharacterRelationType.Spouse))
                            {
                                session.SendPacket($"info {Language.Instance.GetMessageFromKey("ALREADY_MARRIED")}");
                                return;
                            }
                            ClientSession otherSession = ServerManager.Instance.GetSessionByCharacterId(characterId);
                            if (otherSession != null)
                            {
                                otherSession.SendPacket(UserInterfaceHelper.Instance.GenerateDialog(
                                    $"#fins^-34^{session.Character.CharacterId} #fins^-69^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("MARRY_REQUEST"), session.Character.Name)}"));
                                session.Character.FriendRequestCharacters.Add(characterId);
                                //session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);

                            }
                        }
                    }
                    break;

                case 570:
                    if (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.Act4Instance)
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MUST_BE_IN_CLASSIC_MAP"), 0));
                        return;
                    }
                    if (EffectValue < 3)
                    {
                        if (session.Character.Faction == (FactionType)EffectValue)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SAME_FACTION"), 0));
                            return;
                        }
                        session.SendPacket(session.Character.Family == null
                            ? $"qna #guri^750^{EffectValue} {Language.Instance.GetMessageFromKey($"ASK_CHANGE_FACTION{EffectValue}")}"
                            : UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("IN_FAMILY"),
                                0));
                    }
                    else
                    {
                        if (session.Character.Family == null)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAMILY"), 0));
                            return;
                        }
                        if (session.Character.Family.FamilyFaction / 2 == EffectValue)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("SAME_FACTION"), 0));
                            return;
                        }
                        session.SendPacket(session.Character.Family != null
                            ? $"qna #guri^750^{EffectValue} {Language.Instance.GetMessageFromKey($"ASK_CHANGE_FACTION{EffectValue}")}"
                            : UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_IN_FAMILY"),
                                0));
                    }
                    break;

                // wings
                case 650:
                    if (session.Character.UseSp && session.Character.SpInstance != null)
                    {
                        if (option == 0)
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_WINGS_CHANGE")}");
                        }
                        else
                        {
                            session.Character.SpInstance.Design = (byte)EffectValue;
                            session.Character.MorphUpgrade2 = EffectValue;
                            session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                            session.SendPacket(session.Character.GenerateStat());
                            session.SendPacket(session.Character.GenerateStatChar());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"), 0));
                    }
                    break;

                // presentation messages
                case 203:
                    if (!session.Character.IsVehicled)
                    {
                        if (option == 0)
                        {
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateGuri(10, 2, session.Character.CharacterId, 1));
                        }
                    }
                    break;

                // magic lamps
                case 651:
                    if (session.Character.Inventory.All(i => i.Value.Type != InventoryType.Wear))
                    {
                        if (option == 0)
                        {
                            session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^3 {Language.Instance.GetMessageFromKey("ASK_USE")}");
                        }
                        else
                        {
                            session.Character.ChangeSex();
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                    }
                    break;

                // vehicles
                case 1000:
                    if (session.Character.HasShopOpened)
                    {
                        return;
                    }
                    if (Morph > 0)
                    {
                        if (option == 0 && !session.Character.IsVehicled)
                        {
                            if (session.Character.IsSitting)
                            {
                                session.Character.IsSitting = false;
                                session.CurrentMapInstance?.Broadcast(session.Character.GenerateRest());
                            }
                            session.Character.LastDelay = DateTime.Now;
                            session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(3000, 3, $"#u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^2"));
                        }
                        else
                        {
                            if (!session.Character.IsVehicled && option != 0)
                            {
                                DateTime delay = DateTime.Now.AddSeconds(-4);
                                if (session.Character.LastDelay > delay && session.Character.LastDelay < delay.AddSeconds(2))
                                {
                                    session.Character.Speed = Speed;
                                    session.Character.IsVehicled = true;
                                    session.Character.VehicleSpeed = Speed;
                                    session.Character.MorphUpgrade = 0;
                                    session.Character.MorphUpgrade2 = 0;
                                    session.Character.Morph = Morph + (byte)session.Character.Gender;
                                    session.Character.Mates?.ForEach(x => session.CurrentMapInstance?.Broadcast(x.GenerateOut()));
                                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateEff(196), session.Character.MapX, session.Character.MapY);
                                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                                    session.SendPacket(session.Character.GenerateCond());
                                    session.Character.LastSpeedChange = DateTime.Now;
                                }
                            }
                            else if (session.Character.IsVehicled)
                            {
                                session.Character.Mates?.Where(s => s.IsTeamMember).ToList().ForEach(x =>
                                {
                                    x.PositionX = session.Character.PositionX;
                                    x.PositionY = session.Character.PositionY;
                                    session.CurrentMapInstance?.Broadcast(x.GenerateIn());
                                });
                                session.Character.RemoveVehicle();
                            }
                        }
                    }
                    break;

                case 1002:
                    if (session.HasCurrentMapInstance)
                    {
                        if (session.CurrentMapInstance.Map.MapTypes.All(m => m.MapTypeId != (short)MapTypeEnum.Act4))
                        {
                            short[] vnums =
                            {
                                1386, 1387, 1388, 1389, 1390, 1391, 1392, 1393, 1394, 1395, 1396, 1397, 1398, 1399,
                                1400, 1401, 1402, 1403, 1404, 1405
                            };
                            short vnum = vnums[ServerManager.Instance.RandomNumber(0, 20)];

                            NpcMonster npcmonster = ServerManager.Instance.GetNpc(vnum);
                            if (npcmonster == null)
                            {
                                return;
                            }
                            MapMonster monster = new MapMonster
                            {
                                MonsterVNum = vnum,
                                MapY = session.Character.MapY,
                                MapX = session.Character.MapX,
                                MapId = session.Character.MapInstance.Map.MapId,
                                Position = (byte)session.Character.Direction,
                                IsMoving = true,
                                MapMonsterId = session.CurrentMapInstance.GetNextMonsterId(),
                                ShouldRespawn = false
                            };
                            monster.Initialize(session.CurrentMapInstance);
                            session.CurrentMapInstance.AddMonster(monster);
                            session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    break;

                case 69:
                    session.Character.GetReput(ReputPrice);
                    session.SendPacket(session.Character.GenerateFd());
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;

                case 789:
                    session.Character.GiftAdd((short)inv.Item.EffectValue, 1);
                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    break;

                case 1003:
                    if (!session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalSilver))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.BazaarMedalGold
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1004:
                    if (!session.Character.StaticBonusList.Any(s => s.StaticBonusType == StaticBonusType.BazaarMedalGold || s.StaticBonusType == StaticBonusType.BazaarMedalGold))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.BazaarMedalSilver
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1005:
                    if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.BackPack))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.BackPack
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateExts());
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1006:
                    if (option == 0)
                    {
                        session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(byte)inv.Type}^{inv.Slot}^2 {Language.Instance.GetMessageFromKey("ASK_PET_MAX")}");
                    }
                    else
                    {
                        if (session.Character.MaxMateCount < 30)
                        {
                            session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GET_PET_PLACES"), 10));
                            session.SendPacket(session.Character.GenerateScpStc());
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        }
                    }
                    break;

                case 1007:
                    if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.PetBasket))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.PetBasket
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateExts());
                        session.SendPacket("ib 1278 1");
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                case 1008:
                    if (session.Character.StaticBonusList.All(s => s.StaticBonusType != StaticBonusType.PetBackPack))
                    {
                        session.Character.StaticBonusList.Add(new StaticBonusDTO
                        {
                            CharacterId = session.Character.CharacterId,
                            DateEnd = DateTime.Now.AddDays(EffectValue),
                            StaticBonusType = StaticBonusType.PetBackPack
                        });
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateExts());
                        session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("EFFECT_ACTIVATED"), Name), 12));
                    }
                    break;

                default:
                    List<RollGeneratedItemDTO> rolls = inv.Item.RollGeneratedItems?.ToList();
                    if (rolls != null && rolls.Any())
                    {
                        if (option == 0)
                        {
                            if (packetsplit != null && packetsplit.Length == 9 || inv.Item.ItemSubType == 3)
                            {
                                session.SendPacket($"qna #guri^4999^8023^{inv.Slot} {Language.Instance.GetMessageFromKey("ASK_OPEN_BOX")}");
                            }
                            break;
                        }
                        else
                        {
                            int probabilities = rolls.Sum(s => s.Probability);
                            int rnd = ServerManager.Instance.RandomNumber(0, probabilities);
                            int currentrnd = 0;
                            List<ItemInstance> newInv = null;
                            foreach (RollGeneratedItemDTO rollitem in rolls)
                            {
                                if (rollitem.Probability == 10000)
                                {
                                    session.Character.GiftAdd(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount);
                                    continue;
                                }
                                currentrnd += rollitem.Probability;
                                if (newInv != null || currentrnd < rnd)
                                {
                                    continue;
                                }
                                newInv = session.Character.Inventory.AddNewToInventory(rollitem.ItemGeneratedVNum, rollitem.ItemGeneratedAmount, upgrade: rollitem.ItemGeneratedUpgrade);
                                short slot = inv.Slot;
                                if (!newInv.Any() || slot == -1)
                                {
                                    continue;
                                }
                                session.SendPacket(session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newInv.FirstOrDefault()?.Item.Name ?? ""} x {rollitem.ItemGeneratedAmount})", 12));
                                session.SendPacket($"rdi {rollitem.ItemGeneratedVNum} {rollitem.ItemGeneratedAmount}");
                                newInv.ForEach(s => session.SendPacket(s?.GenerateInventoryAdd()));
                            }
                            session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                            break;
                        }
                    }
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType()));
                    break;
            }
            session.Character.IncrementQuests(QuestType.Use, inv.ItemVNum);
        }




        #endregion
    }
}