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

namespace NosSharp.Enums
{
    public enum AuthorityType : short
    {
        Closed = -3,
        Banned = -2,
        Unconfirmed = -1,
        User = 0,
        Vip = 1,
        VipPlus = 3,
        VipPlusPlus = 5,
        Donator = 10,
        DonatorPlus = 15,
        DonatorPlusPlus = 20,
        Moderator = 25,
        GameMaster = 40,
        SuperGameMaster = 50,
        Administrator = 100,
    }
}