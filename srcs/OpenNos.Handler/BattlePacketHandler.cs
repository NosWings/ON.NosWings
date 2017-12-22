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
using OpenNos.Data;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using NosSharp.Enums;
using OpenNos.Core.Extensions;
using OpenNos.Core.Handling;
using OpenNos.GameObject.Buff;
using OpenNos.GameObject.Event.ICEBREAKER;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Packets.ClientPackets;
using OpenNos.GameObject.Battle;

namespace OpenNos.Handler
{
    public class BattlePacketHandler : IPacketHandler
    {
        #region Instantiation

        public BattlePacketHandler(ClientSession session)
        {
            Session = session;
        }

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// mtlist packet
        /// </summary>
        /// <param name="mutliTargetListPacket"></param>
        public void MultiTargetListHit(MultiTargetListPacket mutliTargetListPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                Session.SendPacket("cancel 0 0");
                Session.CurrentMapInstance?.Broadcast(Session.Character.Gender == GenderType.Female
                    ? Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1)
                    : Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                return;
            }
            if ((DateTime.Now - Session.Character.LastTransform).TotalSeconds < 3)
            {
                Session.SendPacket("cancel 0 0");
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACKNOW"), 0));
                return;
            }
            if (Session.Character.IsVehicled)
            {
                Session.SendPacket("cancel 0 0");
                return;
            }
            if (mutliTargetListPacket.TargetsAmount <= 0 || mutliTargetListPacket.TargetsAmount != mutliTargetListPacket.Targets.Count)
            {
                return;
            }
            IEnumerable<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.Select(s => s.Value) : Session.Character.Skills.Select(s => s.Value);
            CharacterSkill ski = null;

            foreach (MultiTargetListSubPacket subpacket in mutliTargetListPacket.Targets)
            {
                IEnumerable<CharacterSkill> characterSkills = skills as IList<CharacterSkill> ?? skills.ToList();
                characterSkills?.FirstOrDefault(s => s.Skill.CastId == subpacket.SkillCastId - 1);
                if (ski == null || !ski.CanBeUsed() || !Session.HasCurrentMapInstance)
                {
                    continue;
                }
                MapMonster mon = Session.CurrentMapInstance.GetMonster(subpacket.TargetId);
                if (mon != null && mon.IsInRange(Session.Character.PositionX, Session.Character.PositionY, ski.Skill.Range) && mon.CurrentHp > 0)
                {
                    Session.Character.LastSkillUse = DateTime.Now;
                    Session.Character.BattleEntity.TargetHit(mon, TargetHitType.SpecialZoneHit, ski.Skill);
                }

                Observable.Timer(TimeSpan.FromMilliseconds(ski.Skill.Cooldown * 100)).Subscribe(o =>
                {
                    Session.SendPacket($"sr {subpacket.SkillCastId - 1}");
                });
            }
            if (ski != null)
            {
                ski.LastUse = DateTime.Now;
            }
        }

        /// <summary>
        /// u_s packet
        /// </summary>
        /// <param name="useSkillPacket"></param>
        public void UseSkill(UseSkillPacket useSkillPacket)
        {
            if (Session.Character.CanFight && useSkillPacket != null)
            {
                PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
                if (Session.Character.IsMuted() && penalty != null)
                {
                    if (Session.Character.Gender == GenderType.Female)
                    {
                        Session.SendPacket("cancel 0 0");
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    }
                    else
                    {
                        Session.SendPacket("cancel 0 0");
                        Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                        Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    }
                    return;
                }
                if (useSkillPacket.MapX.HasValue && useSkillPacket.MapY.HasValue)
                {
                    Session.Character.PositionX = useSkillPacket.MapX.Value;
                    Session.Character.PositionY = useSkillPacket.MapY.Value;
                }
                if (Session.Character.IsSitting)
                {
                    Session.Character.Rest();
                }
                if (Session.Character.IsVehicled || Session.Character.InvisibleGm || Session.Character.HasBuff(BCardType.CardType.SpecialAttack, (byte)AdditionalTypes.SpecialAttack.NoAttack))
                {
                    Session.SendPacket("cancel 0 0");
                    return;
                }
                switch (useSkillPacket.UserType)
                {
                    case UserType.Monster:
                        if (Session.Character.Hp > 0)
                        {
                            TargetHit(useSkillPacket.CastId, useSkillPacket.MapMonsterId);
                        }
                        break;

                    case UserType.Player:
                        if (Session.Character.Hp > 0)
                        {
                            if (useSkillPacket.MapMonsterId != Session.Character.CharacterId)
                            {
                                TargetHit(useSkillPacket.CastId, useSkillPacket.MapMonsterId, true);
                            }
                            else
                            {
                                TargetHit(useSkillPacket.CastId, useSkillPacket.MapMonsterId);
                            }
                        }
                        else
                        {
                            Session.SendPacket("cancel 2 0");
                        }
                        break;

                    default:
                        Session.SendPacket("cancel 2 0");
                        return;
                }
            }
            else
            {
                Session.SendPacket("cancel 2 0");
            }
        }

        /// <summary>
        /// u_as packet
        /// </summary>
        /// <param name="useAoeSkillPacket"></param>
        public void UseZonesSkill(UseAoeSkillPacket useAoeSkillPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
                else
                {
                    Session.SendPacket("cancel 0 0");
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 12));
                }
            }
            else
            {
                if (Session.Character.LastTransform.AddSeconds(3) > DateTime.Now)
                {
                    Session.SendPacket("cancel 0 0");
                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                    return;
                }
                if (Session.Character.IsVehicled)
                {
                    Session.SendPacket("cancel 0 0");
                    return;
                }
                if (Session.Character.CanFight && Session.Character.Hp > 0)
                {
                    ZoneHit(useAoeSkillPacket.CastId, useAoeSkillPacket.MapX, useAoeSkillPacket.MapY);
                }
            }
        }

        private void TargetHit(int castingId, int targetId, bool isPvp = false)
        {
            bool noComboReset = false;
            if ((DateTime.Now - Session.Character.LastTransform).TotalSeconds < 3)
            {
                Session.SendPacket("cancel 0 0");
                Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_ATTACK"), 0));
                return;
            }
            IEnumerable<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp?.Values.ToList() : Session.Character.Skills?.Values.ToList();
            if (skills != null)
            {
                CharacterSkill ski = skills.FirstOrDefault(s => s.Skill?.CastId == castingId && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == 1));
                if (castingId != 0)
                {
                    Session.SendPacket("ms_c 0");
                }
                if (ski != null && (!Session.Character.WeaponLoaded(ski) || !ski.CanBeUsed()))
                {
                    Session.SendPacket($"cancel 2 {targetId}");
                    return;
                }
                if (ski != null)
                {
                    foreach (BCard bc in ski.Skill.BCards.Where(s => s.Type.Equals((byte)BCardType.CardType.MeditationSkill)))
                    {
                        noComboReset = true;
                        bc.ApplyBCards(Session.Character);
                    }
                }

                if (ski != null && Session.Character.Mp >= ski.Skill.MpCost)
                {
                    // AOE Target hit
                    if (ski.Skill.TargetType == 1 && ski.Skill.HitType == 1)
                    {
                        Session.Character.Mp -= Session.Character.HasGodMode ? 0 : ski.Skill.MpCost;

                        if (Session.Character.UseSp && ski.Skill.CastEffect != -1)
                        {
                            Session.SendPackets(Session.Character.GenerateQuicklist());
                        }

                        Session.SendPacket(Session.Character.GenerateStat());
                        CharacterSkill skillinfo = Session.Character.Skills.Select(s => s.Value).OrderBy(o => o.SkillVNum)
                            .FirstOrDefault(s => s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0 && s.Skill.SkillType == 2);
                        Session.CurrentMapInstance?.Broadcast(
                            $"ct 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.CastAnimation} {skillinfo?.Skill.CastEffect ?? ski.Skill.CastEffect} {ski.Skill.SkillVNum}");

                        // Generate scp
                        ski.LastUse = DateTime.Now;
                        if (ski.Skill.CastEffect != 0)
                        {
                            Thread.Sleep(ski.Skill.CastTime * 100);
                        }
                        if (Session.HasCurrentMapInstance)
                        {
                            Session.CurrentMapInstance?.Broadcast(
                                $"su 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.SkillVNum} {ski.Skill.Cooldown} {ski.Skill.AttackAnimation} {skillinfo?.Skill.Effect ?? ski.Skill.Effect} {Session.Character.PositionX} {Session.Character.PositionY} 1 {(int) ((double) Session.Character.Hp / Session.Character.HpLoad() * 100)} 0 -2 {ski.Skill.SkillType - 1}");
                            if (ski.Skill.TargetRange != 0 && Session.HasCurrentMapInstance)
                            {
                                foreach (ClientSession character in ServerManager.Instance.Sessions.Where(s =>
                                    s.CurrentMapInstance == Session.CurrentMapInstance && s.Character.CharacterId != Session.Character.CharacterId &&
                                    s.Character.IsInRange(Session.Character.PositionX, Session.Character.PositionY, ski.Skill.TargetRange)))
                                {
                                    switch (Session.CurrentMapInstance?.MapInstanceType)
                                    {
                                        case MapInstanceType.Act4Instance:
                                            if (Session.CurrentMapInstance.Map.MapId == 130 || Session.CurrentMapInstance.Map.MapId == 131 || Session.Character.Faction == character.Character.Faction)
                                            {
                                                break;
                                            }
                                            Session.Character.BattleEntity.TargetHit(character.Character, TargetHitType.SingleAOETargetHit, ski.Skill, isPvp: true);
                                            break;

                                        case MapInstanceType.IceBreakerInstance:
                                            if (IceBreaker.FrozenPlayers.Contains(character))
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                                Session.Character.LastDelay = DateTime.Now;
                                                Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 3, $"#guri^502^0^{targetId}"));
                                                Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 1, Session.Character.CharacterId), Session.Character.PositionX, Session.Character.PositionY);
                                                return;
                                            }
                                            else if (IceBreaker.SessionsHaveSameGroup(Session, character))
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                                break;
                                            }
                                            break;

                                        default:
                                            if (Session.CurrentMapInstance?.IsPvp == true)
                                            {
                                                if (Session.Character.Group == null || !Session.Character.Group.IsMemberOfGroup(character.Character.CharacterId))
                                                {
                                                    Session.Character.BattleEntity.TargetHit(character.Character, TargetHitType.AOETargetHit, ski.Skill, isPvp: true);
                                                    break;
                                                }
                                                    Session.SendPacket($"cancel 2 {targetId}");
                                                break;
                                            }
                                            Session.SendPacket($"cancel 2 {targetId}");
                                            break;
                                    }
                                }
                                if (Session.CurrentMapInstance != null)
                                {
                                    foreach (MapMonster mon in Session.CurrentMapInstance.GetListMonsterInRange(Session.Character.PositionX, Session.Character.PositionY, ski.Skill.TargetRange)
                                        .Where(s => s.CurrentHp > 0))
                                    {
                                        Session.Character.BattleEntity.TargetHit(mon, TargetHitType.AOETargetHit, ski.Skill, skillinfo?.Skill.Effect ?? ski.Skill.Effect);
                                    }
                                }
                            }
                        }
                    }
                    else if (ski.Skill.TargetType == 2 && ski.Skill.HitType == 0)
                    {
                        Session.CurrentMapInstance?.Broadcast(
                            $"ct 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.CastAnimation} {ski.Skill.CastEffect} {ski.Skill.SkillVNum}");
                        Session.CurrentMapInstance?.Broadcast(
                            $"su 1 {Session.Character.CharacterId} 1 {targetId} {ski.Skill.SkillVNum} {ski.Skill.Cooldown} {ski.Skill.AttackAnimation} {ski.Skill.Effect} {Session.Character.PositionX} {Session.Character.PositionY} 1 {(int) ((double) Session.Character.Hp / Session.Character.HpLoad() * 100)} 0 -1 {ski.Skill.SkillType - 1}");
                        ClientSession target = ServerManager.Instance.GetSessionByCharacterId(targetId) ?? Session;
                        ski.Skill.BCards.ToList().ForEach(s =>
                        {

                            Buff b = new Buff(s.SecondData);

                            switch (b.Card?.CardId)
                            {
                                case (short)BuffType.Good:
                                case (short)BuffType.Neutral:
                                    s.ApplyBCards(target.Character);
                                    break;
                                case (short)BuffType.Bad:
                                    if (!target.Character.HasBuff(BCardType.CardType.NoDefeatAndNoDamage,
                                        (byte)AdditionalTypes.NoDefeatAndNoDamage.TransferAttackPower))
                                    {
                                        s.ApplyBCards(target.Character);
                                    }
                                    break;
                            }
                        });
                    }
                    else if (ski.Skill.TargetType == 1 && ski.Skill.HitType != 1)
                    {
                        Session.CurrentMapInstance?.Broadcast(
                            $"ct 1 {Session.Character.CharacterId} 1 {Session.Character.CharacterId} {ski.Skill.CastAnimation} {ski.Skill.CastEffect} {ski.Skill.SkillVNum}");
                        Session.CurrentMapInstance?.Broadcast(
                            $"su 1 {Session.Character.CharacterId} 1 {targetId} {ski.Skill.SkillVNum} {ski.Skill.Cooldown} {ski.Skill.AttackAnimation} {ski.Skill.Effect} {Session.Character.PositionX} {Session.Character.PositionY} 1 {(int) ((double) Session.Character.Hp / Session.Character.HpLoad() * 100)} 0 -1 {ski.Skill.SkillType - 1}");
                        switch (ski.Skill.HitType)
                        {
                            case 2:
                                IEnumerable<IBattleEntity> entityInRange = Session.CurrentMapInstance?.GetBattleEntitiesInRange(Session.Character.GetPos(), ski.Skill.TargetRange).Where(b => b.SessionType() == SessionType.Character || b.SessionType() == SessionType.MateAndNpc);
                                if (entityInRange != null)
                                {
                                    foreach (IBattleEntity target in entityInRange)
                                    {
                                        if (ski.SkillVNum == 871) // No bcard for this skill
                                        {
                                            List<BuffType> buffsToDisable = new List<BuffType> { BuffType.Bad };
                                            target.BattleEntity.DisableBuffs(buffsToDisable, 4);
                                        }
                                        ski.Skill.BCards.ToList().ForEach(s =>
                                        {
                                            switch (Session.CurrentMapInstance.MapInstanceType)
                                            {
                                                case MapInstanceType.Act4Instance:
                                                    Buff bf = new Buff(s.SecondData);
                                                    switch (bf.Card?.BuffType)
                                                    {
                                                        case BuffType.Bad:
                                                            s.ApplyBCards(target);
                                                            break;
                                                        case BuffType.Good:
                                                        case BuffType.Neutral:
                                                            if (target is Character character && Session.Character.Faction == character.Faction)
                                                            {
                                                                s.ApplyBCards(target);
                                                            }
                                                            break;
                                                    }
                                                    break;
                                                case MapInstanceType.ArenaInstance:
                                                    Buff b = new Buff(s.SecondData);
                                                    switch (b.Card?.BuffType)
                                                    {
                                                        case BuffType.Bad:
                                                            s.ApplyBCards(target);
                                                            break;
                                                        case BuffType.Good:
                                                        case BuffType.Neutral:
                                                            if (target is Character character && Session.Character.Group?.GroupType == GroupType.Group &&
                                                                Session.Character.Group.IsMemberOfGroup(character.CharacterId))
                                                            {
                                                                s.ApplyBCards(target);
                                                            }
                                                            else
                                                            {
                                                                s.ApplyBCards(Session.Character);
                                                            }
                                                            break;
                                                    }
                                                    break;
                                                default:
                                                    s.ApplyBCards(target);
                                                    break;
                                            }
                                        });
                                    }
                                }
                                break;

                            case 4:
                            case 0:
                                ski.Skill.BCards.ToList().ForEach(s =>
                                {
                                    s.ApplyBCards(Session.Character);
                                });
                                break;


                        }
                    }
                    else if (ski.Skill.TargetType == 0 && Session.HasCurrentMapInstance)
                    {
                        if (isPvp)
                        {
                            ClientSession playerToAttack = ServerManager.Instance.GetSessionByCharacterId(targetId);
                            if (Session.CurrentMapInstance?.MapInstanceType == MapInstanceType.IceBreakerInstance)
                            {
                                if (IceBreaker.FrozenPlayers.Contains(playerToAttack))
                                {
                                    Session.SendPacket($"cancel 2 {targetId}");
                                    Session.Character.LastDelay = DateTime.Now;
                                    Session.SendPacket(UserInterfaceHelper.Instance.GenerateDelay(5000, 3, $"#guri^502^0^{targetId}"));
                                    Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 1, Session.Character.CharacterId), Session.Character.PositionX, Session.Character.PositionY);
                                    return;
                                }
                                else if (IceBreaker.SessionsHaveSameGroup(Session, playerToAttack))
                                {
                                    Session.SendPacket($"cancel 2 {targetId}");
                                    return;
                                }
                            }
                            if (playerToAttack != null && Session.Character.Mp >= ski.Skill.MpCost)
                            {
                                if (Map.GetDistance(new MapCell {X = Session.Character.PositionX, Y = Session.Character.PositionY},
                                        new MapCell {X = playerToAttack.Character.PositionX, Y = playerToAttack.Character.PositionY}) <= ski.Skill.Range + 1)
                                {
                                    ski.LastUse = DateTime.Now;
                                    if (!Session.Character.HasGodMode)
                                    {
                                        Session.Character.Mp -= ski.Skill.MpCost;
                                    }
                                    if (Session.Character.UseSp && ski.Skill.CastEffect != -1)
                                    {
                                        Session.SendPackets(Session.Character.GenerateQuicklist());
                                    }
                                    Session.SendPacket(Session.Character.GenerateStat());
                                    CharacterSkill characterSkillInfo = Session.Character.Skills.Select(s => s.Value).OrderBy(o => o.SkillVNum)
                                        .FirstOrDefault(s => s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0 && s.Skill.SkillType == 2);

                                    Session.CurrentMapInstance?.Broadcast(
                                        $"ct 1 {Session.Character.CharacterId} 3 {targetId} {ski.Skill.CastAnimation} {characterSkillInfo?.Skill.CastEffect ?? ski.Skill.CastEffect} {ski.Skill.SkillVNum}");
                                    Session.Character.Skills.Select(s => s.Value).Where(s => s.Id != ski.Id).ToList().ForEach(i => i.Hit = 0);

                                        ski.Hit = (short)((DateTime.Now - ski.LastUse).TotalSeconds > 3 ? 0 : ski.Hit + 1);
                                    ski.LastUse = DateTime.Now;
                                    if (ski.Skill.CastEffect != 0)
                                    {
                                        Thread.Sleep(ski.Skill.CastTime * 100);
                                    }

                                    // check if we will hit mutltiple targets
                                    if (ski.Skill.TargetRange != 0)
                                    {
                                        ComboDTO skillCombo = ski.Skill.Combos.FirstOrDefault(s => ski.Hit == s.Hit);
                                        if (skillCombo != null)
                                        {
                                            if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                            {
                                                ski.Hit = 0;
                                            }
                                            IEnumerable<ClientSession> playersInAoeRange = ServerManager.Instance.Sessions.Where(s =>
                                                s.CurrentMapInstance == Session.CurrentMapInstance && s.Character.CharacterId != Session.Character.CharacterId &&
                                                s.Character.IsInRange(Session.Character.PositionX, Session.Character.PositionY, ski.Skill.TargetRange));
                                            int count = 0;
                                            foreach (ClientSession character in playersInAoeRange)
                                            {
                                                if (Session.CurrentMapInstance == null || !Session.CurrentMapInstance.IsPvp 
                                                    || Session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4Instance && Session.Character.Faction == character.Character.Faction)
                                                {
                                                    continue;
                                                }

                                                ConcurrentBag<ArenaTeamMember> team = null;
                                                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                                                {
                                                    team = ServerManager.Instance.ArenaTeams.FirstOrDefault(s => s.Any(o => o.Session == Session));
                                                }

                                                if ((team == null || team.FirstOrDefault(s => s.Session == Session)?.ArenaTeamType ==
                                                     team.FirstOrDefault(s => s.Session == character)?.ArenaTeamType) &&
                                                    (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance ||
                                                     (Session.Character.Group != null && Session.Character.Group.IsMemberOfGroup(character.Character.CharacterId))))
                                                {
                                                    continue;
                                                }
                                                count++;

                                                Session.Character.BattleEntity.TargetHit(playerToAttack.Character, TargetHitType.SingleTargetHitCombo, ski.Skill, skillCombo: skillCombo, isPvp: true);
                                            }
                                            if (playerToAttack.Character.Hp <= 0 || count == 0)
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }

                                        }
                                        else
                                        {
                                            IEnumerable<ClientSession> playersInAoeRange = ServerManager.Instance.Sessions.Where(s =>
                                                s.CurrentMapInstance == Session.CurrentMapInstance && s.Character.CharacterId != Session.Character.CharacterId &&
                                                s.Character.IsInRange(Session.Character.PositionX, Session.Character.PositionY, ski.Skill.TargetRange));

                                            // hit the targetted monster

                                            if (Session.CurrentMapInstance != null && Session.CurrentMapInstance.IsPvp)
                                            {
                                                ConcurrentBag<ArenaTeamMember> team = null;
                                                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                                                {
                                                    team = ServerManager.Instance.ArenaTeams.FirstOrDefault(s => s.Any(o => o.Session == Session));
                                                }
                                                if (team != null && team.FirstOrDefault(s => s.Session == Session)?.ArenaTeamType !=
                                                    team.FirstOrDefault(s => s.Session == playerToAttack)?.ArenaTeamType
                                                    || Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance &&
                                                    (Session.Character.Group == null || !Session.Character.Group.IsMemberOfGroup(playerToAttack.Character.CharacterId)))
                                                {
                                                    Session.Character.BattleEntity.TargetHit(playerToAttack.Character, TargetHitType.SingleAOETargetHit, ski.Skill, isPvp: true);
                                                }
                                                else
                                                {
                                                    Session.SendPacket($"cancel 2 {targetId}");
                                                }
                                            }
                                            else
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }

                                            //hit all other monsters
                                            foreach (ClientSession character in playersInAoeRange)
                                            {
                                                if (!Session.CurrentMapInstance.IsPvp)
                                                {
                                                    continue;
                                                }
                                                ConcurrentBag<ArenaTeamMember> team = null;
                                                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                                                {
                                                    team = ServerManager.Instance.ArenaTeams.FirstOrDefault(s => s.Any(o => o.Session == Session));
                                                }
                                                if (team != null && team.FirstOrDefault(s => s.Session == Session)?.ArenaTeamType != team.FirstOrDefault(s => s.Session == character)?.ArenaTeamType
                                                    || Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance &&
                                                    (Session.Character.Group == null || !Session.Character.Group.IsMemberOfGroup(character.Character.CharacterId)))
                                                {
                                                    Session.Character.BattleEntity.TargetHit(character.Character, TargetHitType.SingleAOETargetHit, ski.Skill, isPvp: true);
                                                }
                                            }
                                            if (playerToAttack.Character.Hp <= 0)
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ComboDTO skillCombo = ski.Skill.Combos.FirstOrDefault(s => ski.Hit == s.Hit);
                                        if (skillCombo != null)
                                        {
                                            if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                            {
                                                ski.Hit = 0;
                                            }
                                            if (Session.CurrentMapInstance != null && Session.CurrentMapInstance.IsPvp)
                                            {
                                                if (Session.CurrentMapInstance.MapInstanceId != ServerManager.Instance.FamilyArenaInstance.MapInstanceId)
                                                {
                                                    ConcurrentBag<ArenaTeamMember> team = null;
                                                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                                                    {
                                                        team = ServerManager.Instance.ArenaTeams.FirstOrDefault(s => s.Any(o => o.Session == Session));
                                                    }
                                                    if (team != null && team.FirstOrDefault(s => s.Session == Session)?.ArenaTeamType !=
                                                        team.FirstOrDefault(s => s.Session == playerToAttack)?.ArenaTeamType
                                                        || Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance &&
                                                        (Session.Character.Group == null || !Session.Character.Group.IsMemberOfGroup(playerToAttack.Character.CharacterId)))
                                                    {
                                                        Session.Character.BattleEntity.TargetHit(playerToAttack.Character, TargetHitType.SingleTargetHit, ski.Skill, isPvp: true);
                                                    }
                                                    else
                                                    {
                                                        Session.SendPacket($"cancel 2 {targetId}");
                                                    }
                                                }
                                                else
                                                {
                                                    ConcurrentBag<ArenaTeamMember> team = null;
                                                    if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                                                    {
                                                        team = ServerManager.Instance.ArenaTeams.FirstOrDefault(s => s.Any(o => o.Session == Session));
                                                    }
                                                    if (team != null && team.FirstOrDefault(s => s.Session == Session)?.ArenaTeamType !=
                                                        team.FirstOrDefault(s => s.Session == playerToAttack)?.ArenaTeamType
                                                        || Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance &&
                                                        (Session.Character.Group == null || !Session.Character.Group.IsMemberOfGroup(playerToAttack.Character.CharacterId)))
                                                    {
                                                        Session.Character.BattleEntity.TargetHit(playerToAttack.Character, TargetHitType.SingleTargetHit, ski.Skill, isPvp: true);
                                                    }
                                                    else
                                                    {
                                                        Session.SendPacket($"cancel 2 {targetId}");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }
                                        }
                                        else
                                        {
                                            if (Session.CurrentMapInstance != null && Session.CurrentMapInstance.IsPvp)
                                            {
                                                ConcurrentBag<ArenaTeamMember> team = null;
                                                if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
                                                {
                                                    team = ServerManager.Instance.ArenaTeams.FirstOrDefault(s => s.Any(o => o.Session == Session));
                                                }
                                                if (team != null && team.FirstOrDefault(s => s.Session == Session)?.ArenaTeamType !=
                                                    team.FirstOrDefault(s => s.Session == playerToAttack)?.ArenaTeamType
                                                    || Session.CurrentMapInstance.MapInstanceType != MapInstanceType.TalentArenaMapInstance &&
                                                    (Session.Character.Group == null || !Session.Character.Group.IsMemberOfGroup(playerToAttack.Character.CharacterId)))
                                                {
                                                    Session.Character.BattleEntity.TargetHit(playerToAttack.Character, TargetHitType.SingleTargetHit, ski.Skill, isPvp: true);
                                                }
                                                else
                                                {
                                                    Session.SendPacket($"cancel 2 {targetId}");
                                                }
                                            }
                                            else
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Session.SendPacket($"cancel 2 {targetId}");
                                }
                            }
                            else
                            {
                                Session.SendPacket($"cancel 2 {targetId}");
                            }
                        }
                        else
                        {
                            MapMonster monsterToAttack = Session.CurrentMapInstance.GetMonster(targetId);
                            if (monsterToAttack != null && Session.Character.Mp >= ski.Skill.MpCost)
                            {
                                if (Map.GetDistance(new MapCell {X = Session.Character.PositionX, Y = Session.Character.PositionY},
                                        new MapCell {X = monsterToAttack.MapX, Y = monsterToAttack.MapY}) <= ski.Skill.Range + 1 + monsterToAttack.Monster.BasicArea)
                                {
                                    ski.LastUse = DateTime.Now;
                                    if (!Session.Character.HasGodMode)
                                    {
                                        Session.Character.Mp -= ski.Skill.MpCost;
                                    }
                                    if (Session.Character.UseSp && ski.Skill.CastEffect != -1)
                                    {
                                        Session.SendPackets(Session.Character.GenerateQuicklist());
                                    }
                                    monsterToAttack.Monster.BCards.Where(s => s.CastType == 1).ToList().ForEach(s => s.ApplyBCards(monsterToAttack, Session.Character));
                                    Session.SendPacket(Session.Character.GenerateStat());
                                    CharacterSkill characterSkillInfo = Session.Character.Skills.Select(s => s.Value).OrderBy(o => o.SkillVNum)
                                        .FirstOrDefault(s => s.Skill.UpgradeSkill == ski.Skill.SkillVNum && s.Skill.Effect > 0 && s.Skill.SkillType == 2);

                                    Session.CurrentMapInstance?.Broadcast(
                                        $"ct 1 {Session.Character.CharacterId} 3 {monsterToAttack.MapMonsterId} {ski.Skill.CastAnimation} {characterSkillInfo?.Skill.CastEffect ?? ski.Skill.CastEffect} {ski.Skill.SkillVNum}");
                                    Session.Character.Skills.Select(s => s.Value).Where(s => s.Id != ski.Id).ToList().ForEach(i => i.Hit = 0);

                                    // Generate scp

                                    if ((DateTime.Now - ski.LastUse).TotalSeconds > 3)
                                    {
                                        ski.Hit = 0;
                                    }
                                    else
                                    {
                                        ski.Hit++;
                                    }
                                    ski.LastUse = DateTime.Now;
                                    if (ski.Skill.CastEffect != 0)
                                    {
                                        Thread.Sleep(ski.Skill.CastTime * 100);
                                    }

                                    // check if we will hit mutltiple targets
                                    if (ski.Skill.TargetRange != 0)
                                    {
                                        ComboDTO skillCombo = ski.Skill.Combos.FirstOrDefault(s => ski.Hit == s.Hit);
                                        if (skillCombo != null)
                                        {
                                            if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                            {
                                                ski.Hit = 0;
                                            }
                                            IEnumerable<MapMonster> monstersInAoeRange = Session.CurrentMapInstance
                                                ?.GetListMonsterInRange(monsterToAttack.MapX, monsterToAttack.MapY, ski.Skill.TargetRange).Where(s => s.IsFactionTargettable(Session.Character.Faction))
                                                .ToList();
                                            if (monstersInAoeRange != null)
                                            {
                                                foreach (MapMonster mon in monstersInAoeRange)
                                                {
                                                    Session.Character.BattleEntity.TargetHit(mon, TargetHitType.SingleTargetHitCombo, ski.Skill, skillCombo: skillCombo);
                                                }
                                            }
                                            else
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }
                                            if (!monsterToAttack.IsAlive || !monsterToAttack.IsFactionTargettable(Session.Character.Faction))
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }
                                        }
                                        else
                                        {
                                            IEnumerable<MapMonster> monstersInAoeRange = Session.CurrentMapInstance
                                                ?.GetListMonsterInRange(monsterToAttack.MapX, monsterToAttack.MapY, ski.Skill.TargetRange).Where(s => s.IsFactionTargettable(Session.Character.Faction))
                                                .ToList();

                                            //hit the targetted monster
                                            Session.Character.BattleEntity.TargetHit(monsterToAttack, TargetHitType.SingleAOETargetHit, ski.Skill, characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect, showTargetAnimation: true);

                                            //hit all other monsters
                                            if (monstersInAoeRange != null)
                                            {
                                                foreach (MapMonster mon in monstersInAoeRange.Where(m => m.MapMonsterId != monsterToAttack.MapMonsterId)) //exclude targetted monster
                                                {
                                                    Session.Character.BattleEntity.TargetHit(mon, TargetHitType.SingleAOETargetHit, ski.Skill, characterSkillInfo?.Skill.Effect ?? ski.Skill.Effect);
                                                }
                                            }
                                            else
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }
                                            if (!monsterToAttack.IsAlive || !monsterToAttack.IsFactionTargettable(Session.Character.Faction))
                                            {
                                                Session.SendPacket($"cancel 2 {targetId}");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (!monsterToAttack.IsAlive || !monsterToAttack.IsFactionTargettable(Session.Character.Faction))
                                        {
                                            Session.SendPacket($"cancel 2 0");
                                            return;
                                        }
                                        ComboDTO skillCombo = ski.Skill.Combos.FirstOrDefault(s => ski.Hit == s.Hit);
                                        if (skillCombo != null)
                                        {
                                            if (ski.Skill.Combos.OrderByDescending(s => s.Hit).First().Hit == ski.Hit)
                                            {
                                                ski.Hit = 0;
                                            }
                                            Session.Character.BattleEntity.TargetHit(monsterToAttack, TargetHitType.SingleTargetHitCombo, ski.Skill, skillCombo: skillCombo);
                                        }
                                        else
                                        {
                                            Session.Character.BattleEntity.TargetHit(monsterToAttack, TargetHitType.SingleTargetHit, ski.Skill);
                                        }
                                    }
                                }
                                else
                                {
                                    Session.SendPacket($"cancel 2 {targetId}");
                                }
                            }
                            else
                            {
                                Session.SendPacket($"cancel 2 {targetId}");
                            }
                        }
                    }
                    else
                    {
                        Session.SendPacket($"cancel 2 {targetId}");
                    }
                    Session.SendPacketAfter($"sr {castingId}", ski.Skill.Cooldown * 100);
                }
                else
                {
                    Session.SendPacket($"cancel 2 {targetId}");
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                }
                if (castingId < 11 && Session.Character.LastSkillUse.AddSeconds(1) < DateTime.Now && !noComboReset)
                {
                    Session.SendPackets(Session.Character.GenerateQuicklist());
                }
                Session.Character.LastSkillUse = DateTime.Now;
            }
            else
            {
                Session.SendPacket($"cancel 2 {targetId}");
            }
        }

        private void ZoneHit(int castingid, short x, short y)
        {
            IEnumerable<CharacterSkill> skills = Session.Character.UseSp ? Session.Character.SkillsSp.Select(s => s.Value) : Session.Character.Skills.Select(s => s.Value);
            CharacterSkill characterSkill = skills.FirstOrDefault(s => s.Skill.CastId == castingid);
            if (!Session.Character.WeaponLoaded(characterSkill) || !Session.HasCurrentMapInstance)
            {
                Session.SendPacket("cancel 2 0");
                return;
            }

            if (characterSkill != null && characterSkill.CanBeUsed())
            {
                if (Session.Character.Mp >= characterSkill.Skill.MpCost)
                {
                    Session.CurrentMapInstance?.Broadcast($"ct_n 1 {Session.Character.CharacterId} 3 -1 {characterSkill.Skill.CastAnimation} {characterSkill.Skill.CastEffect} {characterSkill.Skill.SkillVNum}");
                    characterSkill.LastUse = DateTime.Now;
                    if (!Session.Character.HasGodMode)
                    {
                        Session.Character.Mp -= characterSkill.Skill.MpCost;
                    }
                    Session.SendPacket(Session.Character.GenerateStat());
                    characterSkill.LastUse = DateTime.Now;
                    Observable.Timer(TimeSpan.FromMilliseconds(characterSkill.Skill.CastTime * 100))
                    .Subscribe(
(Action<long>)(                    o =>
                    {
                        Session.Character.LastSkillUse = DateTime.Now;

                        Session.CurrentMapInstance?.Broadcast($"bs 1 {Session.Character.CharacterId} {x} {y} {characterSkill.Skill.SkillVNum} {characterSkill.Skill.Cooldown} {characterSkill.Skill.AttackAnimation} {characterSkill.Skill.Effect} 0 0 1 1 0 0 0");

                        IEnumerable<MapMonster> monstersInRange = Session.CurrentMapInstance?.GetListMonsterInRange(x, y, characterSkill.Skill.TargetRange).ToList();
                        if (monstersInRange != null)
                        {
                            foreach (MapMonster mon in monstersInRange.Where(s => s.CurrentHp > 0))
                            {
                                Session.Character.BattleEntity.TargetHit(mon, TargetHitType.ZoneHit, characterSkill.Skill, mapX: x, mapY: y);
                            }
                        }
                        foreach (ClientSession character in ServerManager.Instance.Sessions.Where(s => s.CurrentMapInstance == Session.CurrentMapInstance && s.Character.CharacterId != Session.Character.CharacterId && s.Character.IsInRange(x, y, characterSkill.Skill.TargetRange)))
                        {
                            if (Session.CurrentMapInstance == null || !Session.CurrentMapInstance.IsPvp)
                            {
                                continue;
                            }
                            if (Session.Character.Group == null || !Session.Character.Group.IsMemberOfGroup(character.Character.CharacterId))
                            {
                                Session.Character.BattleEntity.TargetHit(character.Character, TargetHitType.ZoneHit, characterSkill.Skill, mapX: x, mapY: y, isPvp: true);
                            }
                        }
                    }));

                    Observable.Timer(TimeSpan.FromMilliseconds(characterSkill.Skill.Cooldown * 100)).Subscribe(o =>
                    {
                        Session.SendPacket($"sr {castingid}");
                    });
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MP"), 10));
                    Session.SendPacket("cancel 2 0");
                }
            }
            else
            {
                Session.SendPacket($"cancel 0 {(characterSkill != null ? castingid : 0)}");
            }
        }

        #endregion
    }
}