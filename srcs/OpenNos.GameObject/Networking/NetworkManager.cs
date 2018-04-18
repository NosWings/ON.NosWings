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
using System.Collections.Generic;
using System.Linq;
using OpenNos.Core;
using OpenNos.Core.Networking;
using OpenNos.Core.Networking.Communication.Scs.Communication.EndPoints.Tcp;
using OpenNos.Core.Networking.Communication.Scs.Server;

namespace OpenNos.GameObject.Networking
{
    public class NetworkManager<TEncryptorT> : SessionManager
    where TEncryptorT : EncryptionBase
    {
        #region Instantiation

        public NetworkManager(string ipAddress, int port, Type packetHandler, Type fallbackEncryptor,
            bool isWorldServer) : base(packetHandler, isWorldServer)
        {
            _encryptor = (TEncryptorT)Activator.CreateInstance(typeof(TEncryptorT));

            if (fallbackEncryptor != null)
            {
                _fallbackEncryptor =
                    (EncryptionBase)Activator.CreateInstance(fallbackEncryptor); // reflection, TODO: optimize.
            }

            _server = ScsServerFactory.CreateServer(new ScsTcpEndPoint(ipAddress, port));

            // Register events of the server to be informed about clients
            _server.ClientConnected += OnServerClientConnected;
            _server.ClientDisconnected += OnServerClientDisconnected;
            _server.WireProtocolFactory = new WireProtocolFactory<TEncryptorT>();

            // Start the server
            _server.Start();

            Logger.Log.Info(Language.Instance.GetMessageFromKey("STARTED"));
        }

        #endregion

        #region Properties

        private IDictionary<string, DateTime> ConnectionLog => _connectionLog ?? (_connectionLog = new Dictionary<string, DateTime>());

        #endregion

        #region Members

        private IDictionary<string, DateTime> _connectionLog;
        private readonly TEncryptorT _encryptor;
        private readonly EncryptionBase _fallbackEncryptor;
        private readonly IScsServer _server;

        #endregion

        #region Methods

        public override void StopServer()
        {
            _server.Stop();
            _server.ClientConnected -= OnServerClientDisconnected;
            _server.ClientDisconnected -= OnServerClientConnected;
        }

        protected override ClientSession IntializeNewSession(INetworkClient client)
        {
            if (!CheckGeneralLog(client))
            {
                Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), client.ClientId);
                client.Initialize(_fallbackEncryptor);
                client.SendPacket("failc 1");
                client.Disconnect();
                return null;
            }

            var session = new ClientSession(client);
            session.Initialize(_encryptor, PacketHandler, IsWorldServer);

            return session;
        }

        private bool CheckGeneralLog(INetworkClient client)
        {
            if (client.IpAddress.Contains("127.0.0.1"))
            {
                return true;
            }

            if (ConnectionLog.Any())
            {
                foreach (KeyValuePair<string, DateTime> item in ConnectionLog.Where(cl =>
                        cl.Key.Contains(client.IpAddress.Split(':')[1]) && (DateTime.Now - cl.Value).TotalSeconds > 3)
                    .ToList())
                {
                    ConnectionLog.Remove(item.Key);
                }
            }

            if (ConnectionLog.Any(c => c.Key.Contains(client.IpAddress.Split(':')[1])))
            {
                return false;
            }

            ConnectionLog.Add(client.IpAddress, DateTime.Now);
            return true;
        }

        private void OnServerClientConnected(object sender, ServerClientEventArgs e)
        {
            AddSession(e.Client as NetworkClient);
        }

        private void OnServerClientDisconnected(object sender, ServerClientEventArgs e)
        {
            RemoveSession(e.Client as NetworkClient);
        }

        #endregion
    }
}