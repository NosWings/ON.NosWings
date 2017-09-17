﻿/*
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
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class BCard : BCardDTO
    {
        #region Methods

        public void ApplyBCards(object session, object caster = null)
        {
            switch ((BCardType.CardType)Type)
            {
                case BCardType.CardType.Buff:
                    if (session.GetType() == typeof(Character))
                    {
                        if (ServerManager.Instance.RandomNumber() < FirstData)
                        {
                            Character character = session as Character;
                            character?.AddBuff(new Buff(SecondData, character.Level));
                        }
                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {
                        if (ServerManager.Instance.RandomNumber() < FirstData)
                        {
                            if (session is MapMonster monster)
                            {
                                monster.AddBuff(!(caster is Character character) ? new Buff(SecondData, 1) : new Buff(SecondData, character.Level));
                            }
                        }
                    }
                    else if (session.GetType() == typeof(MapNpc))
                    {
                    }
                    else if (session.GetType() == typeof(Mate))
                    {
                    }
                    break;

                case BCardType.CardType.Move:
                    if (session.GetType() == typeof(Character))
                    {
                        if (session is Character character)
                        {
                            character.LastSpeedChange = DateTime.Now;
                        }
                        Character o = session as Character;
                        o?.Session.SendPacket(o.GenerateCond());
                    }
                    break;

                case BCardType.CardType.Summons:
                    if (session.GetType() == typeof(Character))
                    {
                    }
                    else if (session.GetType() == typeof(MapMonster))
                    {
                        MapMonster monster = session as MapMonster;
                        ConcurrentBag<MonsterToSummon> summonParameters = new ConcurrentBag<MonsterToSummon>();
                        for (int i = 0; i < FirstData; i++)
                        {
                            if (monster == null)
                            {
                                continue;
                            }

                            short x, y;
                            if (SubType == 11)
                            {
                                x = (short)(i + monster.MapX);
                                y = monster.MapY;
                            }
                            else
                            {
                                x = (short)(ServerManager.Instance.RandomNumber(-3, 3) + monster.MapX);
                                y = (short)(ServerManager.Instance.RandomNumber(-3, 3) + monster.MapY);
                            }
                            summonParameters.Add(new MonsterToSummon((short)SecondData, new MapCell { X = x, Y = y }, -1, true));
                        }
                        int rnd = ServerManager.Instance.RandomNumber();
                        if (rnd <= Math.Abs(ThirdData) || ThirdData == 0)
                        {
                            switch (SubType)
                            {
                                case 31:
                                        EventHelper.Instance.RunEvent(new EventContainer(monster.MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                                    break;
                                default:
                                    if (!monster.OnDeathEvents.Any(s => s.EventActionType == EventActionType.SPAWNMONSTERS))
                                    {
                                        monster.OnDeathEvents.Add(new EventContainer(monster.MapInstance, EventActionType.SPAWNMONSTERS, summonParameters));
                                    }
                                    break;
                            }
                        }
                    }
                    else if (session.GetType() == typeof(MapNpc))
                    {
                    }
                    else if (session.GetType() == typeof(Mate))
                    {
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
                            if (session is Character sess)
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
                                sess.Session?.CurrentMapInstance?.Broadcast(sess.GenerateRc(heal));
                                if (sess.Hp + heal < sess.HPLoad())
                                {
                                    sess.Hp += heal;
                                    change = true;
                                }
                                else
                                {
                                    if (sess.Hp != (int)sess.HPLoad())
                                    {
                                        change = true;
                                    }
                                    sess.Hp = (int)sess.HPLoad();
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
                    if (session.GetType() == typeof(MapMonster))
                    {
                        if (caster is Character)
                        {
                            MapMonster monster = session as MapMonster;
                            Character character = caster as Character;
                            if (monster == null || character == null || monster.Monster.RaceType == 1 && character.MapInstance.MapInstanceType == MapInstanceType.BaseMapInstance)
                            {
                                if (monster.Monster.Level < character.Level)
                                {
                                    if (monster.CurrentHp < (monster.Monster.MaxHP / 2))
                                    {
                                        // Algo 
                                        if (character.Mates.Any(m => m.IsTeamMember == true))
                                        {
                                            // remove current pet
                                        }
                                        monster.MapInstance.DespawnMonster(monster);
                                        NpcMonster mateNpc = ServerManager.Instance.GetNpc(monster.Monster.NpcMonsterVNum);
                                        byte lvl = 0;
                                        lvl += monster.Monster.Level;
                                        lvl -= 10;
                                        if (lvl <= 0)
                                        {
                                            lvl = 1;
                                        }
                                        Mate mate = new Mate(character, mateNpc, lvl, MateType.Pet);
                                        character.Mates.Add(mate);
                                        mate.IsTeamMember = true;
                                        character.Session.SendPacket($"ctl 2 {mate.PetId} 3");
                                        character.MapInstance.Broadcast(mate.GenerateIn());
                                        character.Session.SendPacket(character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("YOU_GET_PET"), mate.Name), 0));
                                        character.Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                                        character.Session.SendPackets(character.GenerateScP());
                                        character.Session.SendPackets(character.GenerateScN());
                                        character.Session.SendPacket(UserInterfaceHelper.Instance.GeneratePClear());
                                        character.Session.SendPackets(character.GenerateScP());
                                        character.Session.SendPackets(character.GenerateScN());
                                        character.Session.SendPacket(character.GeneratePinit());
                                        character.Session.SendPackets(character.GeneratePst());
                                    }
                                    else { /* 50% HP min */ }
                                }
                                else {/* Mob lvl must be less than char lvl */ }
                            }
                        }
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