using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using NosSharp.Enums;
using OpenNos.Core.Handling;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Packets.ClientPackets;

namespace OpenNos.Handler
{
    public class MatePacketHandler : IPacketHandler
    {
        public MatePacketHandler(ClientSession session)
        {
            Session = session;
        }

        private ClientSession Session { get; }


        /// <summary>
        /// u_pet packet
        /// </summary>
        /// <param name="upetPacket"></param>
        public void SpecialSkill(UpetPacket upetPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                return;
            }
            Mate attacker = Session.Character.Mates.FirstOrDefault(x => x.MateTransportId == upetPacket.MateTransportId);
            if (attacker == null)
            {
                return;
            }
            NpcMonsterSkill mateSkill = null;
            if (attacker.Monster.Skills.Any())
            {
                mateSkill = attacker.Monster.Skills.FirstOrDefault(x => x.Rate == 0);
            }
            if (mateSkill == null)
            {
                mateSkill = new NpcMonsterSkill
                {
                    SkillVNum = 200
                };
            }
            if (attacker.IsSitting)
            {
                return;
            }
            switch (upetPacket.TargetType)
            {
                case UserType.Monster:
                    if (attacker.Hp > 0)
                    {
                        MapMonster target = Session?.CurrentMapInstance?.GetMonster(upetPacket.TargetId);
                        AttackMonster(attacker, mateSkill, target);
                    }
                    return;

                case UserType.Npc:
                    return;

                case UserType.Player:
                    return;

                case UserType.Object:
                    return;

                default:
                    return;
            }
        }

        /// <summary>
        /// suctl packet
        /// </summary>
        /// <param name="suctlPacket"></param>
        public void Attack(SuctlPacket suctlPacket)
        {
            PenaltyLogDTO penalty = Session.Account.PenaltyLogs.OrderByDescending(s => s.DateEnd).FirstOrDefault();
            if (Session.Character.IsMuted() && penalty != null)
            {
                if (Session.Character.Gender == GenderType.Female)
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_FEMALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                else
                {
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MUTED_MALE"), 1));
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("MUTE_TIME"), (penalty.DateEnd - DateTime.Now).ToString("hh\\:mm\\:ss")), 11));
                }
                return;
            }
            Mate attacker = Session.Character.Mates.FirstOrDefault(x => x.MateTransportId == suctlPacket.MateTransportId);
            if (attacker == null)
            {
                return;
            }
            if (attacker.IsSitting)
            {
                return;
            }
            IEnumerable<NpcMonsterSkill> mateSkills = attacker.IsUsingSp ? attacker.SpSkills.ToList() : attacker.Monster.Skills;
            if (mateSkills != null)
            {
                NpcMonsterSkill ski = mateSkills.FirstOrDefault(s => s?.Skill?.CastId == suctlPacket.CastId);
                switch (suctlPacket.TargetType)
                {
                    case UserType.Monster:
                        if (attacker.Hp > 0)
                        {
                            MapMonster target = Session?.CurrentMapInstance?.GetMonster(suctlPacket.TargetId);
                            AttackMonster(attacker, ski, target);
                        }
                        return;
                }
            }
        }

        public void AttackMonster(Mate attacker, NpcMonsterSkill skill, MapMonster target)
        {
            if (target == null || attacker == null || !target.IsAlive || skill?.Skill?.MpCost > attacker.Mp)
            {
                return;
            }
            if (skill == null)
            {
                skill = new NpcMonsterSkill
                {
                    SkillVNum = attacker.Monster.BasicSkill
                };
            }
            attacker.LastSkillUse = DateTime.Now;
            attacker.Mp -= skill.Skill == null ? 0 : skill.Skill.MpCost;
            target.Monster.BCards.Where(s => s.CastType == 1).ToList().ForEach(s => s.ApplyBCards(attacker));
            Session.CurrentMapInstance?.Broadcast($"ct 2 {attacker.MateTransportId} 3 {target.MapMonsterId} {skill.Skill?.CastAnimation} {skill.Skill?.CastEffect} {skill.Skill?.SkillVNum}");
            attacker.BattleEntity.TargetHit(target, TargetHitType.SingleTargetHit, skill.Skill);
        }

        public void AttackCharacter(Mate attacker, NpcMonsterSkill skill, Character target)
        {
            
        }

        /// <summary>
        /// psl packet
        /// </summary>
        /// <param name="pslPacket"></param>
        public void Psl(PslPacket pslPacket)
        {
            Mate mate = Session.Character.Mates.FirstOrDefault(x => x.IsTeamMember && x.MateType == MateType.Partner);
            if (mate == null)
            {
                return;
            }
            if (pslPacket.Type == 0)
            {
                if (mate.IsUsingSp)
                {
                    mate.IsUsingSp = false;
                    mate.SpSkills = null;
                    Session.Character.MapInstance.Broadcast(mate.GenerateCMode(-1));
                    Session.SendPacket(mate.GenerateCond());
                    Session.SendPacket(mate.GeneratePski());
                    Session.SendPacket(mate.GenerateScPacket());
                    Session.Character.MapInstance.Broadcast(mate.GenerateOut());
                    Session.Character.MapInstance.Broadcast(mate.GenerateIn());
                    Session.SendPacket(Session.Character.GeneratePinit());
                    //psd 30
                }
                else
                {
                    Session.SendPacket("delay 5000 3 #psl^1 ");
                    Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.Instance.GenerateGuri(2, 2, mate.MateTransportId), mate.PositionX, mate.PositionY);
                }
            }
            else
            {
                if (mate.SpInstance == null)
                {
                    return;                    
                }
                mate.IsUsingSp = true;
                //TODO: update pet skills
                mate.SpSkills = new NpcMonsterSkill[3];
                Session.SendPacket(mate.GenerateCond());
                Session.Character.MapInstance.Broadcast(mate.GenerateCMode(mate.SpInstance.Item.Morph));
                Session.SendPacket(mate.GeneratePski());
                Session.SendPacket(mate.GenerateScPacket());
                Session.Character.MapInstance.Broadcast(mate.GenerateOut());
                Session.Character.MapInstance.Broadcast(mate.GenerateIn());
                Session.SendPacket(Session.Character.GeneratePinit());
                Session.Character.MapInstance.Broadcast(mate.GenerateEff(196));
            }
        }
    }
}
