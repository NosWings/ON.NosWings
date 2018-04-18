using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Utilities;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Map;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Event.BattleRoyale
{
    public class BattleRoyaleManager : Singleton<BattleRoyaleManager>
    {
        #region Members

        private const byte _advertisingMinutes = 5;
        private const byte _registrationSeconds = 30;
        private readonly List<ClientSession> _clientSessions = new List<ClientSession>();
        private MapInstance _mapInstance;

        private readonly string _introductionInfo =
            $"----- BATTLE ROYALE -----\n" +
            "\n" +
            "KILL EVERYONE, BE THE LAST" +
            "\n" +
            "Rules : \n" +
            "RULE NUMBER ONE : NO RULE\n" +
            "\n" +
            "Engaging Awards : \n" +
            "1.000.000 > : 2% Total gain on gold" +
            "5.000.000 > : 10% Total gain on gold + reputation" +
            "15.000.000 > : 25% Total gain on all rewards" +
            "\n" +
            "Rewards : \n" +
            "Gold - Reputation - Items\n" +
            "The higher ranked you are, the higher rewards you get.\n" +
            "Top 1 get 100% of the rewards\n" +
            "Top 2 get 50% of the rewards\n" +
            "Top 3 get 30% of the rewards\n" +
            "Top 4-10 get 10% of the rewards\n" +
            "Others just win reputation";

        #endregion

        #region Properties

        public bool HasStarted { get; }

        public bool IsLocked { get; private set; }

        #endregion

        #region Methods

        public void RegisterSession(ClientSession session)
        {
            if (IsLocked)
            {
                return;
            }

            _clientSessions.Add(session);
        }

        /// <summary>
        ///     Unregister ClientSession
        /// </summary>
        /// <param name="session"></param>
        public void UnregisterSession(ClientSession session)
        {
            _clientSessions.Remove(session);
        }

        /// <summary>
        ///     Unregister ClientSession by sessionId from BattleRoyaleManager
        /// </summary>
        /// <param name="sessionId">
        ///     <see cref="ClientSession" />
        /// </param>
        public void UnregisterSession(long sessionId)
        {
            _clientSessions.Remove(_clientSessions.FirstOrDefault(s => s.SessionId == sessionId));
        }

        public void Initialize(Map.Map map)
        {
            _mapInstance = new MapInstance(map, Guid.NewGuid(), false, MapInstanceType.BattleRoyaleMapInstance,
                new InstanceBag());
        }


        public async void Prepare(bool useTimer)
        {
            if (useTimer)
            {
                Advertise(5);
                await Task.Delay(_advertisingMinutes * 60 * 1000);
            }

            ServerManager.Instance.Broadcast(
                UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BATTLE_ROYAL_OPEN"), 0));
            IsLocked = false;

            await Task.Delay(_registrationSeconds * 1000);

            IsLocked = true;
            foreach (ClientSession session in _clientSessions) // Send Introduction
            {
                session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(_introductionInfo));
                session.SendPacket("gb 3 0 0 0");
                session.SendPacket($"s_memo 6 {Language.Instance.GetMessageFromKey("BATTLE_ROYALE_BANK_MEMO")}");
            }

            await Task.Delay(5000);
            Start();

            void Advertise(int minutesTo)
            {
                for (int i = minutesTo - 1; i > 0; i--)
                {
                    Observable.Timer(TimeSpan.FromMinutes(i)).Subscribe(s =>
                    {
                        ServerManager.Instance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                            string.Format(Language.Instance.GetMessageFromKey("BATTLE_ROYAL_REGISTRATION"), i), 0));
                    });
                }
            }
        }

        public async void Start()
        {
            ServerManager.Instance.StartedEvents.Remove(EventType.BATTLEROYAL);
            _clientSessions.ForEach(s =>
                ServerManager.Instance.TeleportOnRandomPlaceInMap(s, _mapInstance.MapInstanceId));
            await Task.Delay(5000);

            if (_mapInstance.Sessions.Count() < 3)
            {
                _mapInstance.Broadcast(
                    UserInterfaceHelper.Instance.GenerateMsg(
                        Language.Instance.GetMessageFromKey("BATTLE_ROYAL_NOT_ENOUGH_PLAYERS"), 0));
                await Task.Delay(3000);
                _mapInstance.Dispose();
                return;
            }

            _mapInstance.Broadcast(
                UserInterfaceHelper.Instance.GenerateMsg(Language.Instance.GetMessageFromKey("BATTLE_ROYAL_STARTED"),
                    0));
            _mapInstance.IsPvp = true;
        }

        public async void Stop()
        {
            NotifyEnd();
            await Task.Delay(10000);
            _mapInstance.Dispose();

            void NotifyEnd()
            {
                _mapInstance.IsPvp = false;
                _clientSessions.ForEach(s => { s.SendPacket("say 1 0 10 [BATTLE ROYALE] There is no winner !"); });
            }
        }

        public void Join(ClientSession session)
        {
            RegisterSession(session);
            // NOTIFY JOINING
        }

        public void Kick(ClientSession kickedSession, ClientSession killerSession = null)
        {
            UnregisterSession(kickedSession);
            if (killerSession == null)
            {
                return;
            }

            GetRewards(kickedSession, _clientSessions.Count + 1);
            _mapInstance.Broadcast(UserInterfaceHelper.Instance.GenerateMsg(
                string.Format(Language.Instance.GetMessageFromKey("BR_KILL"), kickedSession?.Character?.Name,
                    killerSession?.Character?.Name, _clientSessions.Count), 0));
        }

        public void GetRewards(ClientSession session, int index)
        {
            if (index < 5)
            {
                switch (index)
                {
                    case 1:
                        break;

                    case 2:
                        break;

                    case 3:
                        break;

                    case 4:
                        break;
                }
            }
            else if (index < 10)
            {
                session.Character.GetReput(15000, true);
            }
            else
            {
                session.Character.GetReput(5000, true);
            }
        }

        #endregion
    }
}