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

using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Item.Instance;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Item
{
    public class NoFunctionItem : Item
    {
        #region Instantiation

        public NoFunctionItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte option = 0,
            string[] packetsplit = null)
        {
            switch (Effect)
            {
                case 10:
                    const short gillionVNum = 1013;
                    const short cellaVNum = 1014;
                    short[] cristalItems = {1028, 1029, 1031, 1032, 1033, 1034};
                    short[] cellonItems = {1017, 1018, 1019};
                    short[] soulGemItems = {1015, 1016};

                    int extraItems = ServerManager.Instance.RandomNumber(0, 101);

                    if (session.Character.Inventory.CountItem(gillionVNum) <= 0)
                    {
                        // No Gillion                   
                        session.SendPacket(
                            session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_GILLION"), 11));
                        return;
                    }

                    session.Character.GiftAdd(cellaVNum, (byte) ServerManager.Instance.RandomNumber(5, 11));
                    if (extraItems > 70)
                    {
                        switch ((RefinerType) EffectValue)
                        {
                            case RefinerType.SoulGem:
                                session.Character.GiftAdd(
                                    soulGemItems[ServerManager.Instance.RandomNumber(0, soulGemItems.Length)], 1);
                                break;
                            case RefinerType.Cellon:
                                session.Character.GiftAdd(
                                    cellonItems[ServerManager.Instance.RandomNumber(0, cellonItems.Length)], 1);
                                break;
                            case RefinerType.Crystal:
                                session.Character.GiftAdd(
                                    cristalItems[ServerManager.Instance.RandomNumber(0, cellonItems.Length)], 1);
                                break;
                        }
                    }

                    session.Character.Inventory.RemoveItemAmount(gillionVNum);
                    session.Character.Inventory.RemoveItemAmount(VNum);
                    break;
                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType()));
                    break;
            }
        }

        #endregion
    }
}