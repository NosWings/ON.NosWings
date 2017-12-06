using System;
using System.Collections.Generic;
using OpenNos.Master.Library.Data;
using NosSharp.CLI.Interfaces;

namespace NosSharp.Mediator
{
    public class MasterMediator
    {
        #region Members

        private Dictionary<Guid, ICliClient> _clients;

        #endregion

        #region Instanciation

        /// <summary>
        /// Instanciate a new MasterMediator
        /// </summary>
        public MasterMediator()
        {
            _clients = new Dictionary<Guid, ICliClient>();
            _worldServers = new Dictionary<Guid, SerializableWorldServer>();
            _sessions = new Dictionary<long, AccountSession>();
        }

        #endregion

        #region Singleton

        private static MasterMediator _instance;

        /// <summary>
        /// Singleton
        /// </summary>
        public static MasterMediator Instance
        {
            get { return _instance ?? (_instance = new MasterMediator()); }
        }

        #endregion

        #region Sessions

        /// <summary>
        /// Sessions container, key is AccountId
        /// </summary>
        private readonly Dictionary<long, AccountSession> _sessions;

        /// <summary>
        /// /
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public bool RegisterSession(AccountSession session)
        {
            return _sessions.ContainsKey(session.AccountId) || _sessions.TryAdd(session.AccountId, session);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public void UnregisterSession(AccountSession session)
        {
            if (session == null)
            {
                return;
            }
            _sessions.Remove(session.AccountId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountId"></param>
        public void UnregisterSessionByAccountId(long accountId)
        {
            _sessions.Remove(accountId);
        }

        #endregion

        #region WorldServers

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<Guid, SerializableWorldServer> _worldServers;

        /// <summary>
        /// Register a new WorldServer
        /// </summary>
        /// <param name="worldServer"></param>
        /// <returns>New WorldServer GUID</returns>
        public Guid RegisterWorldServer(SerializableWorldServer worldServer)
        {
            Guid newGuid = Guid.NewGuid();
            _worldServers.TryAdd(newGuid, worldServer);
            return newGuid;
        }

        /// <summary>
        /// Unregister a new WorldServer
        /// </summary>
        /// <param name="id"></param>
        /// <returns>New WorldServer GUID</returns>
        public void UnregisterWorldServerByGuid(Guid id)
        {
            _worldServers.Remove(id);
        }

        #endregion
    }
}