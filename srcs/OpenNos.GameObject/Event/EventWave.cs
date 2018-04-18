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

namespace OpenNos.GameObject.Event
{
    public class EventWave
    {
        public EventWave(byte delay, ConcurrentBag<EventContainer> events, byte offset = 0)
        {
            Delay = delay;
            Offset = offset;
            Events = events;
        }

        public byte Delay { get; }
        public byte Offset { get; set; }
        public DateTime LastStart { get; set; }
        public ConcurrentBag<EventContainer> Events { get; }

        #region Methods

        #endregion
    }
}