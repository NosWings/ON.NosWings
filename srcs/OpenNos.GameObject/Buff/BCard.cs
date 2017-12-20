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
using System.Linq;
using System.Reactive.Linq;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.GameObject.Battle;
using System.Collections.Generic;
using OpenNos.Core.Extensions;

namespace OpenNos.GameObject.Buff
{
    public class BCard : BCardDTO
    {
        #region Methods

        public void ApplyBCards(IBattleEntity session, IBattleEntity caster = null)
        {
            switch ((BCardType.CardType)Type)
            {
                case BCardType.CardType.Buff:
                    if (ServerManager.Instance.RandomNumber() < FirstData)
                    {
                        session?.BattleEntity.AddBuff(new Buff(SecondData, caster.BattleEntity.Level));
                    }
                    break;

                case BCardType.CardType.Move:
                    if (session.GetSession() is Character chara)
                    {
                        chara.LastSpeedChange = DateTime.Now;
                        chara.LoadSpeed();
                        chara?.Session.SendPacket(chara.GenerateCond());
                    }
                    break;

                case BCardType.CardType.Summons:
                    NpcMonster npcMonster = session.GetSession() is MapMonster mob ? mob.Monster : session.GetSession() is MapNpc npc ? npc.Npc : null;
                    ConcurrentBag<ToSummon> summonParameters = new ConcurrentBag<ToSummon>();

                    switch ((AdditionalTypes.Summons)SubType)
                    {
                        case AdditionalTypes.Summons.Summons:
                            for (int i = 0; i < FirstData; i++)
                            {
                                MapCell cell = session.GetPos();
                                cell.Y += (short)ServerManager.Instance.RandomNumber(-3, 3);
                                cell.X += (short)ServerManager.Instance.RandomNumber(-3, 3);
                                summonParameters.Add(new ToSummon((short)SecondData, cell, null, true, (byte)Math.Abs(ThirdData)));
                            }
                            EventHelper.Instance.RunEvent(new EventContainer(session.MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                            break;

                        case AdditionalTypes.Summons.SummonTrainingDummy:
                            if (npcMonster != null && session.BattleEntity.OnHitEvents.All(s => s?.EventActionType != EventActionType.SPAWNMONSTERS))
                            {
                                summonParameters.Add(new ToSummon((short)SecondData, session.GetPos(), null, true, (byte)Math.Abs(ThirdData)));
                                session.BattleEntity.OnHitEvents.Add(new EventContainer(session.MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                            }
                            break;

                        case AdditionalTypes.Summons.SummonUponDeathChance:
                        case AdditionalTypes.Summons.SummonUponDeath:
                            if (npcMonster != null && session.BattleEntity.OnDeathEvents.All(s => s?.EventActionType != EventActionType.SPAWNMONSTERS))
                            {
                                for (int i = 0; i < FirstData; i++)
                                {
                                    MapCell cell = session.GetPos();
                                    cell.Y += (short)i;
                                    summonParameters.Add(new ToSummon((short)SecondData, cell, null, true, (byte)Math.Abs(ThirdData)));
                                }
                                session.BattleEntity.OnDeathEvents.Add(new EventContainer(session.MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                            }
                            break;

                        default:
                            break;
                    }
                    break;

                case BCardType.CardType.SpecialAttack:
                    break;

                case BCardType.CardType.SpecialDefence:
                    break;

                case BCardType.CardType.AttackPower:
                    break;

                case BCardType.CardType.Target:
                    break;

                case BCardType.CardType.Critical:
                    break;

                case BCardType.CardType.SpecialCritical:
                    break;

                case BCardType.CardType.Element:
                    break;

                case BCardType.CardType.IncreaseDamage:
                    break;

                case BCardType.CardType.Defence:
                    break;

                case BCardType.CardType.DodgeAndDefencePercent:
                    break;

                case BCardType.CardType.Block:
                    break;

                case BCardType.CardType.Absorption:
                    break;

                case BCardType.CardType.ElementResistance:
                    break;

                case BCardType.CardType.EnemyElementResistance:
                    break;

                case BCardType.CardType.Damage:
                    break;

                case BCardType.CardType.GuarantedDodgeRangedAttack:
                    break;

                case BCardType.CardType.Morale:
                    break;

                case BCardType.CardType.Casting:
                    break;

                case BCardType.CardType.Reflection:
                    break;

                case BCardType.CardType.DrainAndSteal:
                    break;

                case BCardType.CardType.HealingBurningAndCasting:
                    AdditionalTypes.HealingBurningAndCasting subtype = (AdditionalTypes.HealingBurningAndCasting) SubType;
                    switch (subtype)
                    {
                        case AdditionalTypes.HealingBurningAndCasting.RestoreHP:
                        case AdditionalTypes.HealingBurningAndCasting.RestoreHPWhenCasting:
                            if (session.GetSession() is Character sess)
                            {
                                int heal = FirstData;
                                bool change = false;
                                if (IsLevelScaled)
                                {
                                    if (IsLevelDivided)
                                    {
                                        heal /= sess.Level;
                                    }
                                    else
                                    {
                                        heal *= sess.Level;
                                    }
                                }
                                if (sess.Hp + heal < sess.HpLoad())
                                {
                                    sess.Hp += heal;
                                    sess.Session?.CurrentMapInstance?.Broadcast(sess.GenerateRc(heal));
                                    change = true;
                                }
                                else
                                {
                                    if (sess.Hp != (int)sess.HpLoad())
                                    {
                                        sess.Session?.CurrentMapInstance?.Broadcast(sess.GenerateRc((int)(sess.HpLoad() - sess.Hp)));
                                        change = true;
                                    }
                                    sess.Hp = (int)sess.HpLoad();
                                }
                                if (change)
                                {
                                    sess.Session?.SendPacket(sess.GenerateStat());
                                }
                            }
                            break;
                    }
                    break;

                case BCardType.CardType.HPMP:
                    break;

                case BCardType.CardType.SpecialisationBuffResistance:
                    break;

                case BCardType.CardType.SpecialEffects:
                    break;

                case BCardType.CardType.Capture:
                    if (session is MapMonster monsterToCapture && caster is Character hunter)
                    {
                        if (monsterToCapture.Monster.RaceType == 1 && (hunter.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance || hunter.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance))
                        {
                            if (monsterToCapture.Monster.Level < hunter.Level)
                            {
                                if (monsterToCapture.CurrentHp < (monsterToCapture.Monster.MaxHP / 2))
                                {
                                    if (hunter.MaxMateCount > hunter.Mates.Count())
                                    {
                                        // Algo  
                                        int capturerate = 100 - (monsterToCapture.CurrentHp / monsterToCapture.Monster.MaxHP + 1) / 2;
                                        if (ServerManager.Instance.RandomNumber() <= capturerate)
                                        {
                                            if (hunter.Quests.Any(q => q.Quest.QuestType == (int)QuestType.Capture1 && q.Data.Any(d => d.Value[0] == monsterToCapture.MonsterVNum)))
                                            {
                                                // Algo  
                                                int capturerate = 100 - (monster.CurrentHp / monster.Monster.MaxHP + 1) / 2;
                                                if (ServerManager.Instance.RandomNumber() <= capturerate)
                                                {
                                                    if (character.Quests.Any(q => q.Quest.QuestType == (int) QuestType.Capture1 && q.Quest.QuestObjectives.Any(d => d.Data == monster.MonsterVNum)))
                                                    {
                                                        character.IncrementQuests(QuestType.Capture1, monster.MonsterVNum);
                                                        return;
                                                    }
                                                    character.IncrementQuests(QuestType.Capture2, monster.MonsterVNum);

                                            Mate currentmate = hunter.Mates?.FirstOrDefault(m => m.IsTeamMember && m.MateType == MateType.Pet);
                                            if (currentmate != null)
                                            {
                                                currentmate.RemoveTeamMember(); // remove current pet
                                                hunter.MapInstance.Broadcast(currentmate.GenerateOut());
                                            }
                                            monsterToCapture.MapInstance.DespawnMonster(monsterToCapture);
                                            NpcMonster mateNpc = ServerManager.Instance.GetNpc(monsterToCapture.MonsterVNum);
                                            Mate mate = new Mate(hunter, mateNpc, (byte)level, MateType.Pet);
                                            hunter.Mates.Add(mate);
                                            mate.RefreshStats();
                                            hunter.Session.SendPacket($"ctl 2 {mate.PetId} 3");
                                            hunter.MapInstance.Broadcast(mate.GenerateIn());
                                            hunter.Session.SendPacket(hunter.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("YOU_GET_PET"), mate.Name), 0));
                                            hunter.Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                                            hunter.Session.SendPackets(hunter.GenerateScP());
                                            hunter.Session.SendPackets(hunter.GenerateScN());
                                            hunter.Session.SendPacket(hunter.GeneratePinit());
                                            hunter.Session.SendPackets(hunter.Mates.Where(s => s.IsTeamMember)
                                                .OrderBy(s => s.MateType)
                                                .Select(s => s.GeneratePst()));
                                        }
                                        else { hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("CAPTURE_FAILED"), 0)); }
                                    }
                                    else { hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_MATES_COUNT"), 0)); }
                                }
                                else { hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("monsterToCapture_MUST_BE_LOW_HP"), 0)); }
                            }
                            else { hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("monsterToCapture_LVL_MUST_BE_LESS"), 0)); }
                        }
                        else { hunter.Session.SendPacket(UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("monsterToCapture_CANNOT_BE_CAPTURED"), 0)); }
                    }
                    break;

                case BCardType.CardType.SpecialDamageAndExplosions:
                    break;

                case BCardType.CardType.SpecialEffects2:
                    break;

                case BCardType.CardType.CalculatingLevel:
                    break;

                case BCardType.CardType.Recovery:
                    break;

                case BCardType.CardType.MaxHPMP:
                    break;

                case BCardType.CardType.MultAttack:
                    break;

                case BCardType.CardType.MultDefence:
                    break;

                case BCardType.CardType.TimeCircleSkills:
                    break;

                case BCardType.CardType.RecoveryAndDamagePercent:
                    break;

                case BCardType.CardType.Count:
                    break;

                case BCardType.CardType.NoDefeatAndNoDamage:
                    break;

                case BCardType.CardType.SpecialActions:
                    if (session.GetSession() is Character charact)
                    {
                        if (SubType.Equals((byte) AdditionalTypes.SpecialActions.Hide))
                        {
                            charact.Invisible = true;
                            charact.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s => charact.Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                            charact.Session.CurrentMapInstance?.Broadcast(charact.GenerateInvisible());
                        }
                    }
                    break;

                case BCardType.CardType.Mode:
                    break;

                case BCardType.CardType.NoCharacteristicValue:
                    break;

                case BCardType.CardType.LightAndShadow:
                    break;

                case BCardType.CardType.Item:
                    break;

                case BCardType.CardType.DebuffResistance:
                    break;

                case BCardType.CardType.SpecialBehaviour:
                    break;

                case BCardType.CardType.Quest:
                    break;

                case BCardType.CardType.SecondSPCard:
                    break;

                case BCardType.CardType.SPCardUpgrade:
                    break;

                case BCardType.CardType.HugeSnowman:
                    break;

                case BCardType.CardType.Drain:
                    break;

                case BCardType.CardType.BossMonstersSkill:
                    break;

                case BCardType.CardType.LordHatus:
                    break;

                case BCardType.CardType.LordCalvinas:
                    break;

                case BCardType.CardType.SESpecialist:
                    break;

                case BCardType.CardType.FourthGlacernonFamilyRaid:
                    break;

                case BCardType.CardType.SummonedMonsterAttack:
                    break;

                case BCardType.CardType.BearSpirit:
                    break;

                case BCardType.CardType.SummonSkill:
                    break;

                case BCardType.CardType.InflictSkill:
                    break;

                case BCardType.CardType.HideBarrelSkill:
                    break;

                case BCardType.CardType.FocusEnemyAttentionSkill:
                    break;

                case BCardType.CardType.TauntSkill:
                    break;

                case BCardType.CardType.FireCannoneerRangeBuff:
                    break;

                case BCardType.CardType.VulcanoElementBuff:
                    break;

                case BCardType.CardType.DamageConvertingSkill:
                    break;

                case BCardType.CardType.MeditationSkill:
                    if (session.GetSession().GetType() == typeof(Character))
                    {
                        if (SubType.Equals((byte) AdditionalTypes.MeditationSkill.CausingChance))
                        {
                            if (ServerManager.Instance.RandomNumber() < FirstData)
                            {
                                Character character = (session as Character);
                                if (character == null)
                                {
                                    break;
                                }
                                if (SkillVNum.HasValue)
                                {
                                    character.LastSkillCombo = DateTime.Now;
                                    Skill skill = ServerManager.Instance.GetSkill(SkillVNum.Value);
                                    Skill newSkill = ServerManager.Instance.GetSkill((short) SecondData);
                                    Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(observer =>
                                    {
                                        foreach (QuicklistEntryDTO qe in character.QuicklistEntries.Where(s =>
                                            s.Pos.Equals(skill.CastId)))
                                        {
                                            character.Session.SendPacket(
                                                $"qset {qe.Q1} {qe.Q2} {qe.Type}.{qe.Slot}.{newSkill.CastId}.0");
                                        }
                                        character.Session.SendPacket($"mslot {newSkill.CastId} -1");
                                    });

                                    if (skill.CastId > 10)
                                    {
                                        // HACK this way
                                        Observable.Timer(TimeSpan.FromMilliseconds(skill.Cooldown * 100 + 500))
                                            .Subscribe(observer =>
                                            {
                                                character.Session.SendPacket($"sr {skill.CastId}");
                                            });
                                    }
                                }
                            }
                        }
                        else
                        {
                            Character character = (session as Character);
                            if (character == null)
                            {
                                break;
                            }
                            switch (SubType)
                            {
                                case 21:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(4);
                                    break;
                                case 31:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(8);
                                    break;
                                case 41:
                                    character.MeditationDictionary[(short)SecondData] = DateTime.Now.AddSeconds(12);
                                    break;
                            }
                        }
                    }
                    break;

                case BCardType.CardType.FalconSkill:
                    break;

                case BCardType.CardType.AbsorptionAndPowerSkill:
                    break;

                case BCardType.CardType.LeonaPassiveSkill:
                    break;

                case BCardType.CardType.FearSkill:
                    break;

                case BCardType.CardType.SniperAttack:
                    break;

                case BCardType.CardType.FrozenDebuff:
                    break;

                case BCardType.CardType.JumpBackPush:
                    break;

                case BCardType.CardType.FairyXPIncrease:
                    break;

                case BCardType.CardType.SummonAndRecoverHP:
                    break;

                case BCardType.CardType.TeamArenaBuff:
                    break;

                case BCardType.CardType.ArenaCamera:
                    break;

                case BCardType.CardType.DarkCloneSummon:
                    break;

                case BCardType.CardType.AbsorbedSpirit:
                    break;

                case BCardType.CardType.AngerSkill:
                    break;

                case BCardType.CardType.MeteoriteTeleport:
                    break;

                case BCardType.CardType.StealBuff:
                    break;

                default:
                    Logger.Error(new ArgumentOutOfRangeException($"Card Type {Type} not defined!"));
                    //throw new ArgumentOutOfRangeException();
                    break;
            }
        }

        public override void Initialize()
        {
        }

        #endregion
    }
}