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

using System.Collections.Generic;
using OpenNos.GameObject.Event;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Battle;
using System.Collections.Concurrent;
using System;


namespace OpenNos.GameObject
{
    public class ToSummon
    {
        #region Instantiation

        public ToSummon(short vnum, MapCell spawnCell, IBattleEntity target, bool move, byte summonChance = 100,
            bool isTarget = false, bool isBonusOrProtected = false, bool isHostile = true, bool isBossOrMate = false)
        {
            VNum = vnum;
            SpawnCell = spawnCell;
            Target = target;
            IsTarget = isTarget;
            IsMoving = move;
            IsBonusOrProtected = isBonusOrProtected;
            IsBossOrMate = isBossOrMate;
            IsHostile = isHostile;
            SummonChance = (byte) (summonChance == 0 ? 100 : summonChance);
            DeathEvents = new ConcurrentBag<EventContainer>();
            NoticingEvents = new ConcurrentBag<EventContainer>();
        }

        #endregion

        #region Properties

        public byte SummonChance { get; set; }

        public ConcurrentBag<EventContainer> DeathEvents { get; set; }

        public ConcurrentBag<EventContainer> NoticingEvents { get; set; }

        public bool IsBonusOrProtected { get; set; }

        public bool IsBossOrMate { get; set; }

        public bool IsHostile { get; set; }

        public bool IsProtected { get; }

        public bool IsMoving { get; set; }

        public bool IsTarget { get; set; }

        public MapCell SpawnCell { get; set; }

        public IBattleEntity Target { get; set; }

        public short VNum { get; set; }

        public byte NoticeRange { get; set; }

        #endregion
    }
}