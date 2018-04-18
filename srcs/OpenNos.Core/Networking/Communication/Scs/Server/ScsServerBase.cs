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
using System.Linq;
using OpenNos.Core.Networking.Communication.Scs.Communication.Channels;
using OpenNos.Core.Networking.Communication.Scs.Communication.Protocols;

namespace OpenNos.Core.Networking.Communication.Scs.Server
{
    /// <summary>
    ///     This class provides base functionality for server Classs.
    /// </summary>
    public abstract class ScsServerBase : IScsServer, IDisposable
    {
        #region Instantiation

        /// <summary>
        ///     Constructor.
        /// </summary>
        protected ScsServerBase()
        {
            Clients = new ConcurrentDictionary<long, IScsServerClient>();
            WireProtocolFactory = WireProtocolManager.GetDefaultWireProtocolFactory();
        }

        #endregion

        #region Members

        /// <summary>
        ///     This object is used to listen incoming connections.
        /// </summary>
        private IConnectionListener _connectionListener;

        private bool _disposed;

        #endregion

        #region Events

        /// <summary>
        ///     This event is raised when a new client is connected.
        /// </summary>
        public event EventHandler<ServerClientEventArgs> ClientConnected;

        /// <summary>
        ///     This event is raised when a client disconnected from the server.
        /// </summary>
        public event EventHandler<ServerClientEventArgs> ClientDisconnected;

        #endregion

        #region Properties

        /// <summary>
        ///     A collection of clients that are connected to the server.
        /// </summary>
        public ConcurrentDictionary<long, IScsServerClient> Clients { get; }

        /// <summary>
        ///     Gets/sets wire protocol that is used while reading and writing messages.
        /// </summary>
        public IScsWireProtocolFactory WireProtocolFactory { get; set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        /// <summary>
        ///     Starts the server.
        /// </summary>
        public virtual void Start()
        {
            _connectionListener = CreateConnectionListener();
            _connectionListener.CommunicationChannelConnected += connectionListener_CommunicationChannelConnected;
            _connectionListener.Start();
        }

        /// <summary>
        ///     Stops the server.
        /// </summary>
        public virtual void Stop()
        {
            _connectionListener?.Stop();
            foreach (IScsServerClient client in Clients.Select(s => s.Value))
            {
                client.Disconnect();
            }
        }

        /// <summary>
        ///     This method is implemented by derived Classs to create appropriate connection listener to
        ///     listen incoming connection requets.
        /// </summary>
        /// <returns></returns>
        protected abstract IConnectionListener CreateConnectionListener();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Clients.Clear();
            }
        }

        /// <summary>
        ///     Raises ClientConnected event.
        /// </summary>
        /// <param name="client">Connected client</param>
        protected virtual void OnClientConnected(IScsServerClient client) => ClientConnected?.Invoke(this, new ServerClientEventArgs(client));

        /// <summary>
        ///     Raises ClientDisconnected event.
        /// </summary>
        /// <param name="client">Disconnected client</param>
        protected virtual void OnClientDisconnected(IScsServerClient client) => ClientDisconnected?.Invoke(this, new ServerClientEventArgs(client));

        /// <summary>
        ///     Handles Disconnected events of all connected clients.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void client_Disconnected(object sender, EventArgs e)
        {
            var client = (IScsServerClient)sender;
            Clients.TryRemove(client.ClientId, out IScsServerClient value);
            OnClientDisconnected(client);
        }

        /// <summary>
        ///     Handles CommunicationChannelConnected event of _connectionListener object.
        /// </summary>
        /// <param name="sender">Source of event</param>
        /// <param name="e">Event arguments</param>
        private void connectionListener_CommunicationChannelConnected(object sender, CommunicationChannelEventArgs e)
        {
            var client = new NetworkClient(e.Channel)
            {
                ClientId = ScsServerManager.GetClientId(),
                WireProtocol = WireProtocolFactory.CreateWireProtocol()
            };

            client.Disconnected += client_Disconnected;
            Clients[client.ClientId] = client;
            OnClientConnected(client);
            e.Channel.Start();
        }

        #endregion
    }
}