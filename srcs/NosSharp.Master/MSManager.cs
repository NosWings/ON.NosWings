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

using System.Collections.Concurrent;
using System.Collections.Generic;
using Hik.Communication.ScsServices.Service;
using OpenNos.Master.Library.Data;

namespace OpenNos.Master
{
    internal class MsManager
    {
        #region Members

        private static MsManager _instance;


        #endregion

        #region Instantiation

        public MsManager()
        {
            WorldServers = new List<WorldServer>();
            LoginServers = new List<IScsServiceClient>();
            ConnectedAccounts = new ConcurrentBag<AccountSession>();
            AuthentificatedClients = new List<long>();
        }

        #endregion

        #region Properties

        public static MsManager Instance
        {
            get { return _instance ?? (_instance = new MsManager()); }
        }

        public List<long> AuthentificatedClients { get; set; }
        
        public ConcurrentBag<AccountSession> ConnectedAccounts { get; set; }

        public List<IScsServiceClient> LoginServers { get; set; }

        public List<WorldServer> WorldServers { get; set; }

        #endregion
    }
}