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
using System.Linq;
using NosSharp.Enums;
using OpenNos.Data;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Item
{
    public class PotionItem : Item
    {
        #region Instantiation

        public PotionItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0,
            string[] packetsplit = null)
        {
            if ((DateTime.Now - session.Character.LastPotion).TotalMilliseconds < 750)
            {
                return;
            }

            if (session.CurrentMapInstance?.Map.MapId == 153 &&
                (DateTime.Now - session.Character.LastPotion).TotalSeconds < 3)
            {
                return;
            }

            if (session.CurrentMapInstance?.MapInstanceType == MapInstanceType.Act4Instance &&
                (DateTime.Now - session.Character.LastPotion).TotalSeconds < 5)
            {
                return;
            }

            session.Character.LastPotion = DateTime.Now;
            if (session.Character.Hp == session.Character.HpLoad() &&
                session.Character.Mp == session.Character.MpLoad())
            {
                return;
            }

            if (session.Character.Hp <= 0)
            {
                return;
            }

            switch (VNum)
            {
                // Full HP
                case 1242:
                case 5582:
                    if (session.CurrentMapInstance?.MapInstanceType != MapInstanceType.Act4Instance &&
                        session.CurrentMapInstance?.IsPvp != true)
                    {
                        session.CurrentMapInstance?.Broadcast(
                            session.Character.GenerateRc((int)session.Character.HpLoad() - session.Character.Hp));
                        session.Character.Hp = (int)session.Character.HpLoad();
                        foreach (Mate mate in session.Character.Mates.Where(m => m.IsTeamMember))
                        {
                            mate.Hp = mate.HpLoad();
                            session.CurrentMapInstance?.Broadcast(mate.GenerateRc(mate.HpLoad() - mate.Hp));
                        }

                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateStat());
                    }

                    if (session.CurrentMapInstance?.Map.MapId == 153)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(
                            (int)(session.Character.HpLoad() / 2 + session.Character.Hp) >
                            (int)session.Character.HpLoad()
                                ? (int)session.Character.HpLoad() - session.Character.Hp
                                : (int)session.Character.HpLoad() / 2));
                        session.Character.Hp =
                            (int)(session.Character.HpLoad() / 2 + session.Character.Hp) >
                            (int)session.Character.HpLoad()
                                ? (int)session.Character.HpLoad()
                                : session.Character.Hp + (int)session.Character.HpLoad() / 2;
                        foreach (Mate mate in session.Character.Mates.Where(m => m.IsTeamMember))
                        {
                            mate.Hp = mate.HpLoad() / 2 + mate.Hp > mate.HpLoad()
                                ? mate.HpLoad()
                                : mate.Hp + mate.HpLoad() / 2;
                            session.CurrentMapInstance?.Broadcast(mate.GenerateRc(
                                mate.HpLoad() / 2 + mate.Hp > mate.HpLoad()
                                    ? mate.HpLoad() - mate.Hp
                                    : mate.HpLoad() / 2));
                        }

                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateStat());
                    }

                    break;

                // Full MP
                case 1243:
                case 5583:
                    if (session.CurrentMapInstance?.MapInstanceType != MapInstanceType.Act4Instance &&
                        session.CurrentMapInstance?.IsPvp != true)
                    {
                        session.Character.Mp = (int)session.Character.MpLoad();
                        session.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m => m.Mp = m.MpLoad());
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateStat());
                    }

                    if (session.CurrentMapInstance?.Map.MapId == 153)
                    {
                        session.Character.Mp =
                            (int)(session.Character.MpLoad() / 2 + session.Character.Mp) >
                            (int)session.Character.MpLoad()
                                ? (int)session.Character.MpLoad()
                                : session.Character.Mp + (int)session.Character.MpLoad() / 2;
                        session.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m =>
                            m.Mp = m.MpLoad() / 2 + m.Mp > m.MpLoad() ? m.MpLoad() : m.Mp + m.MpLoad() / 2);
                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateStat());
                    }

                    break;

                // Full Mp & Hp
                case 1244:
                case 5584:
                    if (session.CurrentMapInstance?.MapInstanceType != MapInstanceType.Act4Instance &&
                        session.CurrentMapInstance?.IsPvp != true)
                    {
                        session.CurrentMapInstance?.Broadcast(
                            session.Character.GenerateRc((int)session.Character.HpLoad() - session.Character.Hp));
                        session.Character.Hp = (int)session.Character.HpLoad();
                        session.Character.Mp = (int)session.Character.MpLoad();
                        foreach (Mate mate in session.Character.Mates.Where(m => m.IsTeamMember))
                        {
                            mate.Hp = mate.HpLoad();
                            mate.Mp = mate.MpLoad();
                            session.CurrentMapInstance?.Broadcast(mate.GenerateRc(mate.HpLoad() - mate.Hp));
                        }

                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateStat());
                    }

                    if (session.CurrentMapInstance?.Map.MapId == 153)
                    {
                        session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(
                            (int)(session.Character.HpLoad() / 2 + session.Character.Hp) >
                            (int)session.Character.HpLoad()
                                ? (int)session.Character.HpLoad() - session.Character.Hp
                                : (int)session.Character.HpLoad() / 2));
                        session.Character.Hp =
                            (int)(session.Character.HpLoad() / 2 + session.Character.Hp) >
                            (int)session.Character.HpLoad()
                                ? (int)session.Character.HpLoad()
                                : session.Character.Hp + (int)session.Character.HpLoad() / 2;
                        session.Character.Mp =
                            (int)(session.Character.MpLoad() / 2 + session.Character.Mp) >
                            (int)session.Character.MpLoad()
                                ? (int)session.Character.MpLoad()
                                : session.Character.Mp + (int)session.Character.MpLoad() / 2;
                        foreach (Mate mate in session.Character.Mates.Where(m => m.IsTeamMember))
                        {
                            mate.Hp = mate.HpLoad() / 2 + mate.Hp > mate.HpLoad()
                                ? mate.HpLoad()
                                : mate.Hp + mate.HpLoad() / 2;
                            mate.Mp = mate.MpLoad() / 2 + mate.Mp > mate.MpLoad()
                                ? mate.MpLoad()
                                : mate.Mp + mate.MpLoad() / 2;
                            session.CurrentMapInstance?.Broadcast(mate.GenerateRc(
                                mate.HpLoad() / 2 + mate.Hp > mate.HpLoad()
                                    ? mate.HpLoad() - mate.Hp
                                    : mate.HpLoad() / 2));
                        }

                        session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                        session.SendPacket(session.Character.GenerateStat());
                    }

                    break;

                default:
                    int hpHeal = session.Character.Hp + Hp > session.Character.HpLoad()
                        ? (int)session.Character.HpLoad() - session.Character.Hp
                        : Hp;
                    session.CurrentMapInstance?.Broadcast(session.Character.GenerateRc(hpHeal));
                    session.Character.Hp += hpHeal;
                    session.Character.Mp += session.Character.Mp + Mp > session.Character.MpLoad()
                        ? (int)session.Character.MpLoad() - session.Character.Mp
                        : Mp;

                    foreach (Mate mate in session.Character.Mates.Where(m => m.IsTeamMember))
                    {
                        int mateHpHeal = mate.Hp + Hp > mate.HpLoad() ? mate.HpLoad() - mate.Hp : Hp;
                        mate.Hp += mateHpHeal;
                        mate.Mp += mate.Mp + Mp > mate.MpLoad() ? mate.MpLoad() : Mp;
                        session.CurrentMapInstance?.Broadcast(mate.GenerateRc(mateHpHeal));
                    }

                    session.Character.Inventory.RemoveItemAmountFromInventory(1, inv.Id);
                    session.SendPacket(session.Character.GenerateStat());
                    break;
            }
        }

        #endregion
    }
}