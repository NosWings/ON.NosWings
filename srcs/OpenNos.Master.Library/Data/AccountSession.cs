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

namespace OpenNos.Master.Library.Data
{
    public class AccountSession
    {
        #region Instantiation

        public AccountSession(long accountId, long session, string accountName)
        {
            AccountId = accountId;
            SessionId = session;
            LastPulse = DateTime.Now;
            AccountName = accountName;
        }

        #endregion

        #region Properties

        public string AccountName { get; }

        public long AccountId { get; }

        public long CharacterId { get; set; }

        public CharacterSession Character { get; set; }

        public bool CanSwitchChannel { get; set; }

        public DateTime LastPulse { get; set; }

        public WorldServer ConnectedWorld { get; set; }

        public WorldServer PreviousChannel { get; set; }

        public long SessionId { get; }

        #endregion

        #region CharacterSession

        public class CharacterSession
        {
            public CharacterSession(string name, int level, string gender, string @class)
            {
                Name = name;
                Level = level;
                Gender = gender;
                Class = @class;
            }

            public string Name { get; }
            public int Level { get; }
            public string Gender { get; }
            public string Class { get; }
        }

        #endregion
    }
}