using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
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
        private MapInstance _mapInstance;
        private bool _started;

        private readonly string _introductionInfo = $"----- BATTLE ROYALE -----\n" +
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

        private readonly List<ClientSession> _clientSessions = new List<ClientSession>();

        public bool HasStarted
        {
            get { return _started; }
        }

        public void RegisterSession(ClientSession session)
        {
            _clientSessions.Add(session);
        }

        /// <summary>
        /// Unregister ClientSession
        /// </summary>
        /// <param name="session"></param>
        public void UnregisterSession(ClientSession session)
        {
            _clientSessions.Remove(session);
        }

        /// <summary>
        /// Unregister ClientSession by sessionId from BattleRoyaleManager
        /// </summary>
        /// <param name="sessionId"><see cref="ClientSession"/></param>
        public void UnregisterSession(long sessionId)
        {
        }

        public void Initialize(Map.Map map)
        {
            _mapInstance = new MapInstance(map, Guid.NewGuid(), false, MapInstanceType.BattleRoyaleMapInstance, new InstanceBag());
        }

        public void Prepare()
        {
            Advertise(5);
            WaitAndLock(30);
            SendIntroduction();

            void Advertise(long minutesTo)
            {
                for (long i = minutesTo - 1; i > 0; i--)
                {
                    Observable.Timer(TimeSpan.FromMinutes(i)).Subscribe(s => { ServerManager.Instance.Broadcast($"say 1 0 10 [BATTLE ROYALE] Registration will be opened in {minutesTo}"); });
                }
            }

            void WaitAndLock(long delay)
            {
                Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(s => { _started = true; });
            }

            void SendIntroduction()
            {
                foreach (ClientSession session in _clientSessions)
                {
                    session.SendPacket(UserInterfaceHelper.Instance.GenerateInfo(_introductionInfo));
                    session.SendPacket($"gb 3 0 0 0");
                    session.SendPacket($"s_memo 6 {Language.Instance.GetMessageFromKey("BATTLE_ROYALE_BANK_MEMO")}");
                }
            }
        }

        public void Start()
        {
            _mapInstance.IsPvp = true;
        }

        public void Stop()
        {
            NotifyEnd();


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

        public void Kick(ClientSession kickedSession, ClientSession killerSession)
        {
            UnregisterSession(kickedSession);
            _clientSessions.ForEach(s =>
            {
                s.SendPacket(kickedSession.Character.GenerateSay($"[BR] : {killerSession} killed {kickedSession}", 11));
            });
        }
    }
}