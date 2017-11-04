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

using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Service;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.Master.Library.Data;
using OpenNos.Master.Library.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using OpenNos.Data;

namespace OpenNos.Master.Server
{
    internal class CommunicationService : ScsService, ICommunicationService
    {
        #region Instantiation

        public CommunicationService()
        {
        }

        #endregion

        #region Methods

        public bool Authenticate(string authKey)
        {
            if (string.IsNullOrWhiteSpace(authKey) || authKey != ConfigurationManager.AppSettings["MasterAuthKey"])
            {
                return false;
            }
            MSManager.Instance.AuthentificatedClients.Add(CurrentClient.ClientId);
            return true;
        }

        public void Cleanup()
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts.Clear();
            MSManager.Instance.WorldServers.Clear();
        }

        public bool ConnectAccount(Guid worldId, long accountId, long sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if (account != null)
            {
                account.ConnectedWorld = MSManager.Instance.WorldServers.FirstOrDefault(w => w.Id.Equals(worldId));
            }
            return account?.ConnectedWorld != null;
        }

        public bool ConnectCharacter(Guid worldId, long characterId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }

            //Multiple WorldGroups not yet supported by DAOFactory
            long accountId = DaoFactory.CharacterDao.LoadById(characterId)?.AccountId ?? 0;

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.ConnectedWorld?.Id.Equals(worldId) == true);
            CharacterDTO character = DaoFactory.CharacterDao.LoadById(characterId);
            if (account == null || character == null)
            {
                return false;
            }
            account.CharacterId = characterId;
            account.Character = new AccountConnection.CharacterSession(character.Name, character.Level, character.Gender.ToString(), character.Class.ToString());
            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().CharacterConnected(characterId);
            }
            Console.Title = $"MASTER SERVER - Channels :{MSManager.Instance.WorldServers.Count} - Players : {MSManager.Instance.ConnectedAccounts.Count(s => s.Character != null)}";
            return true;
        }

        public SerializableWorldServer GetAct4ChannelInfo(string worldGroup)
        {
            WorldServer act4Channel = MSManager.Instance.WorldServers.FirstOrDefault(s => s.IsAct4 && s.WorldGroup == worldGroup);

            if (act4Channel != null)
            {
                return act4Channel.Serializable;
            }
            act4Channel = MSManager.Instance.WorldServers.FirstOrDefault(s => s.WorldGroup == worldGroup);
            if (act4Channel == null)
            {
                return null;
            }
            Logger.Log.Info($"[{act4Channel.WorldGroup}] ACT4 Channel elected on ChannelId : {act4Channel.ChannelId} ");
            act4Channel.IsAct4 = true;
            return act4Channel.Serializable;
        }

        public bool IsCrossServerLoginPermitted(long accountId, int sessionId)
        {
            return MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MSManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.CanSwitchChannel);
        }

        public void DisconnectAccount(long accountId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            if (MSManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.CanSwitchChannel))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(s => s.AccountId != accountId);
        }

        public void DisconnectCharacter(Guid worldId, long characterId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (AccountConnection account in MSManager.Instance.ConnectedAccounts.Where(c => c.CharacterId.Equals(characterId) && c.ConnectedWorld.Id.Equals(worldId)))
            {
                foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(account.ConnectedWorld.WorldGroup)))
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().CharacterDisconnected(characterId);
                }
                if (account.CanSwitchChannel)
                {
                    continue;
                }
                account.Character = null;
                account.ConnectedWorld = null;
                Console.Title = $"MASTER SERVER - Channels :{MSManager.Instance.WorldServers.Count} - Players : {MSManager.Instance.ConnectedAccounts.Count(s => s.CharacterId != 0)}";
            }
        }

        public int? GetChannelIdByWorldId(Guid worldId)
        {
            return MSManager.Instance.WorldServers.FirstOrDefault(w => w.Id == worldId)?.ChannelId;
        }

        public bool IsAccountConnected(long accountId)
        {
            return MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MSManager.Instance.ConnectedAccounts.Any(c => c.AccountId == accountId && c.ConnectedWorld != null);
        }

        public bool IsCharacterConnected(string worldGroup, long characterId)
        {
            return MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MSManager.Instance.ConnectedAccounts.Any(c => c.ConnectedWorld != null && c.ConnectedWorld.WorldGroup == worldGroup && c.CharacterId == characterId);
        }

        public bool IsLoginPermitted(long accountId, long sessionId)
        {
            return MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)) &&
                MSManager.Instance.ConnectedAccounts.Any(s => s.AccountId.Equals(accountId) && s.SessionId.Equals(sessionId) && s.ConnectedWorld == null);
        }

        public void KickSession(long? accountId, long? sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().KickSession(accountId, sessionId);
            }
            if (accountId.HasValue)
            {
                MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(s => !s.AccountId.Equals(accountId.Value));
            }
            else if (sessionId.HasValue)
            {
                MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(s => !s.SessionId.Equals(sessionId.Value));
            }
        }

        public void RefreshPenalty(int penaltyId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers)
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }
            foreach (IScsServiceClient login in MSManager.Instance.LoginServers)
            {
                login.GetClientProxy<ICommunicationClient>().UpdatePenaltyLog(penaltyId);
            }
        }

        public void RegisterAccountLogin(long accountId, long sessionId, string accountName)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(a => !a.AccountId.Equals(accountId));
            MSManager.Instance.ConnectedAccounts.Add(new AccountConnection(accountId, sessionId, accountName));
        }

        public void RegisterInternalAccountLogin(long accountId, int sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));

            if (account != null)
            {
                account.CanSwitchChannel = true;
            }
        }

        public bool ConnectAccountInternal(Guid worldId, long accountId, int sessionId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return false;
            }
            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId) && a.SessionId.Equals(sessionId));
            if (account == null)
            {
                return false;
            }
            {
                account.CanSwitchChannel = false;
                account.PreviousChannel = account.ConnectedWorld;
                account.ConnectedWorld = MSManager.Instance.WorldServers.FirstOrDefault(s => s.Id.Equals(worldId));
                if (account.ConnectedWorld != null)
                {
                    return true;
                }
            }
            return false;
        }

        public SerializableWorldServer GetPreviousChannelByAccountId(long accountId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.AccountId.Equals(accountId));
            return account?.PreviousChannel?.Serializable;
        }

        public int? RegisterWorldServer(SerializableWorldServer worldServer)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }
            WorldServer ws = new WorldServer(worldServer.Id, new ScsTcpEndPoint(worldServer.EndPointIp, worldServer.EndPointPort), worldServer.AccountLimit, worldServer.WorldGroup)
            {
                ServiceClient = CurrentClient,
                ChannelId = Enumerable.Range(1, 30).Except(MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldServer.WorldGroup)).OrderBy(w => w.ChannelId).Select(w => w.ChannelId))
                    .First(),
                Serializable = new SerializableWorldServer(worldServer.Id, worldServer.EndPointIp, worldServer.EndPointPort, worldServer.AccountLimit, worldServer.WorldGroup),
                IsAct4 = false
            };
            MSManager.Instance.WorldServers.Add(ws);
            return ws.ChannelId;
        }

        public string RetrieveRegisteredWorldServers(long sessionId)
        {
            string lastGroup = string.Empty;
            byte worldCount = 0;
            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.SessionId == sessionId);
            if (account == null)
            {
                return null;
            }
            string channelPacket = $"NsTeST {account.AccountName} {sessionId} ";
            foreach (WorldServer world in MSManager.Instance.WorldServers.OrderBy(w => w.WorldGroup))
            {
                if (lastGroup != world.WorldGroup)
                {
                    worldCount++;
                }
                lastGroup = world.WorldGroup;

                int currentlyConnectedAccounts = MSManager.Instance.ConnectedAccounts.Count(a => a.ConnectedWorld?.ChannelId == world.ChannelId);
                int channelcolor = (int)Math.Round((double)currentlyConnectedAccounts / world.AccountLimit * 20) + 1;

                channelPacket += $"{world.Endpoint.IpAddress}:{world.Endpoint.TcpPort}:{channelcolor}:{worldCount}.{world.ChannelId}.{world.WorldGroup} ";
            }
            channelPacket += "-1:-1:-1:10000.10000.1";
            return MSManager.Instance.WorldServers.Any() ? channelPacket : null;
        }

        public string RetrieveServerStatistics()
        {
            Dictionary<int, List<AccountConnection.CharacterSession>> dictionary =
                MSManager.Instance.WorldServers.ToDictionary(world => world.ChannelId, world => new List<AccountConnection.CharacterSession>());

            foreach (IGrouping<int, AccountConnection> accountConnections in MSManager.Instance.ConnectedAccounts.GroupBy(s => s.ConnectedWorld.ChannelId))
            {
                foreach (AccountConnection i in accountConnections)
                {
                    dictionary[accountConnections.Key].Add(i.Character);
                }
            }
            return JsonConvert.SerializeObject(dictionary);
        }

        public int? SendMessageToCharacter(SCSCharacterMessage message)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return null;
            }

            WorldServer sourceWorld = MSManager.Instance.WorldServers.FirstOrDefault(s => s.Id.Equals(message.SourceWorldId));
            if (message?.Message == null || sourceWorld == null)
            {
                return null;
            }
            switch (message.Type)
            {
                case MessageType.Family:
                case MessageType.FamilyChat:
                    foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(sourceWorld.WorldGroup)))
                    {
                        world.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;

                case MessageType.PrivateChat:
                case MessageType.Whisper:
                case MessageType.WhisperGM:
                    if (message.DestinationCharacterId.HasValue)
                    {
                        AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.CharacterId.Equals(message.DestinationCharacterId.Value));
                        if (account?.ConnectedWorld != null)
                        {
                            account.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                            return account.ConnectedWorld.ChannelId;
                        }
                    }
                    break;

                case MessageType.Shout:
                    foreach (WorldServer world in MSManager.Instance.WorldServers)
                    {
                        world.ServiceClient.GetClientProxy<ICommunicationClient>().SendMessageToCharacter(message);
                    }
                    return -1;
            }
            return null;
        }

        public void UnregisterWorldServer(Guid worldId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            MSManager.Instance.ConnectedAccounts = MSManager.Instance.ConnectedAccounts.Where(a => a == null || a.ConnectedWorld?.Id.Equals(worldId) != true);
            MSManager.Instance.WorldServers.RemoveAll(w => w.Id.Equals(worldId));
        }

        public void UpdateBazaar(string worldGroup, long bazaarItemId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateBazaar(bazaarItemId);
            }
        }

        public void UpdateFamily(string worldGroup, long familyId, bool changeFaction)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateFamily(familyId, changeFaction);
            }
        }

        public void Shutdown(string worldGroup)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            if (worldGroup == "*")
            {
                foreach (WorldServer world in MSManager.Instance.WorldServers)
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
            else
            {
                foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
                {
                    world.ServiceClient.GetClientProxy<ICommunicationClient>().Shutdown();
                }
            }
        }

        public void UpdateRelation(string worldGroup, long relationId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }

            foreach (WorldServer world in MSManager.Instance.WorldServers.Where(w => w.WorldGroup.Equals(worldGroup)))
            {
                world.ServiceClient.GetClientProxy<ICommunicationClient>().UpdateRelation(relationId);
            }
        }

        public void PulseAccount(long accountId)
        {
            if (!MSManager.Instance.AuthentificatedClients.Any(s => s.Equals(CurrentClient.ClientId)))
            {
                return;
            }
            AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.AccountId.Equals(accountId));
            if (account != null)
            {
                account.LastPulse = DateTime.Now;
            }
        }

        public void CleanupOutdatedSession()
        {
            AccountConnection[] tmp = new AccountConnection[MSManager.Instance.ConnectedAccounts.Count + 20];
            lock(MSManager.Instance.ConnectedAccounts)
            {
                MSManager.Instance.ConnectedAccounts.ToList().CopyTo(tmp);
            }
            foreach (AccountConnection account in tmp.Where(a => a != null && a.LastPulse.AddMinutes(5) <= DateTime.Now))
            {
                KickSession(account.AccountId, null);
            }
        }

        public bool ChangeAuthority(string worldGroup, string characterName, AuthorityType authority)
        {
            CharacterDTO character = DaoFactory.CharacterDao.LoadByName(characterName);
            if (character == null)
            {
                return false;
            }
            if (!IsAccountConnected(character.AccountId))
            {
                AccountDTO account = DaoFactory.AccountDao.LoadById(character.AccountId);
                account.Authority = authority;
                DaoFactory.AccountDao.InsertOrUpdate(ref account);
            }
            else
            {
                AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(s => s.AccountId == character.AccountId);
                account.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().ChangeAuthority(account.AccountId, authority);
            }
            return true;
        }

        public void SendMail(string worldGroup, MailDTO mail)
        {
            if (!IsCharacterConnected(worldGroup, mail.ReceiverId))
            {
                CharacterDTO chara = DaoFactory.CharacterDao.LoadById(mail.ReceiverId);
                DaoFactory.MailDao.InsertOrUpdate(ref mail);
            }
            else
            {
                AccountConnection account = MSManager.Instance.ConnectedAccounts.FirstOrDefault(a => a.CharacterId.Equals(mail.ReceiverId));
                if (account == null || account.ConnectedWorld == null)
                {
                    DaoFactory.MailDao.InsertOrUpdate(ref mail);
                    return;
                }
                account.ConnectedWorld.ServiceClient.GetClientProxy<ICommunicationClient>().SendMail(mail);
            }
        }

        #endregion
    }
}