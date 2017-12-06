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
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Networking;

namespace OpenNos.GameObject.Networking
{
    public class SessionManager
    {
        #region Members

        protected Type PacketHandler { get; }

        protected ConcurrentDictionary<long, ClientSession> Sessions = new ConcurrentDictionary<long, ClientSession>();

        #endregion

        #region Instantiation

        public SessionManager(Type packetHandler, bool isWorldServer)
        {
            PacketHandler = packetHandler;
            IsWorldServer = isWorldServer;
        }

        #endregion

        #region Properties

        public bool IsWorldServer { get; set; }

        #endregion

        #region Methods

        public void AddSession(INetworkClient customClient)
        {
            Logger.Log.Info(Language.Instance.GetMessageFromKey("NEW_CONNECT") + customClient.ClientId);

            ClientSession session = IntializeNewSession(customClient);
            customClient.SetClientSession(session);

            if (session == null || !IsWorldServer)
            {
                return;
            }
            if (Sessions.TryAdd(customClient.ClientId, session))
            {
                return;
            }
            Logger.Log.WarnFormat(Language.Instance.GetMessageFromKey("FORCED_DISCONNECT"), customClient.ClientId);
            customClient.Disconnect();
            Sessions.TryRemove(customClient.ClientId, out session);
        }

        public virtual void StopServer()
        {
            Sessions.Clear();
            ServerManager.Instance.StopServer();
        }

        protected virtual ClientSession IntializeNewSession(INetworkClient client)
        {
            ClientSession session = new ClientSession(client);
            client.SetClientSession(session);
            return session;
        }

        protected void RemoveSession(INetworkClient client)
        {
            Sessions.TryRemove(client.ClientId, out ClientSession session);

            // check if session hasnt been already removed
            if (session != null)
            {
                session.IsDisposing = true;

                if (IsWorldServer && session.HasSelectedCharacter)
                {
                    session.Character.Mates.Where(s => s.IsTeamMember).ToList().ForEach(s => session.CurrentMapInstance?.Broadcast(session, s.GenerateOut(), ReceiverType.AllExceptMe));
                    session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateOut(), ReceiverType.AllExceptMe);
                }

                session.Destroy();

                if (IsWorldServer)
                {
                    if (session.HasSelectedCharacter)
                    {
                        if (session.Character.Hp < 1)
                        {
                            session.Character.Hp = 1;
                        }

                        if (ServerManager.Instance.Groups.Any(s => s.IsMemberOfGroup(session.Character.CharacterId)))
                        {
                            ServerManager.Instance.GroupLeave(session);
                        }
                        session.Character.LeaveTalentArena(true);
                        session.Character.Save();
                    }
                }

                client.Disconnect();
                Logger.Log.Info(Language.Instance.GetMessageFromKey("DISCONNECT") + client.ClientId);
                // session = null;
            }
        }

        #endregion
    }
}