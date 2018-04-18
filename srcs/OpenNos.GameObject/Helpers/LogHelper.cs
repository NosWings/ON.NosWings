using System;
using System.Collections.Generic;
using NosSharp.Enums;
using OpenNos.Core;
using OpenNos.Core.Serializing;
using OpenNos.Data;
using OpenNos.DAL;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Helpers
{
    public class LogHelper
    {
        public LogHelper()
        {
            QuestLogList = new List<QuestLogDTO>();
            RaidLogList = new List<RaidLogDTO>();
            LogCommandsList = new List<LogCommandsDTO>();
            ChatLogList = new List<LogChatDTO>();
        }

        #region Properties
        
        public List<QuestLogDTO> QuestLogList;
        public List<RaidLogDTO> RaidLogList;
        public List<LogCommandsDTO> LogCommandsList;
        public List<LogChatDTO> ChatLogList;
        #endregion

        public void InsertCommandLog(long characterId, PacketDefinition commandPacket, string ipAddress)
        {
            string withoutHeaderpacket = string.Empty;
            string[] packet = commandPacket.OriginalContent.Split(' ');
            for (int i = 1; i < packet.Length; i++)
            {
                withoutHeaderpacket += $" {packet[i]}";
            }

            LogCommandsDTO command = new LogCommandsDTO
            {
                CharacterId = characterId,
                Command = commandPacket.OriginalHeader,
                Data = withoutHeaderpacket,
                IpAddress = ipAddress,
                Timestamp = DateTime.Now
            };
            LogCommandsList.Add(command);
            if (LogCommandsList.Count != 500)
            {
                return;
            }
            DaoFactory.LogCommandsDao.InsertOrUpdateList(ref LogCommandsList);
            LogCommandsList.Clear();
        }

        public void InsertChatLog(ChatType type, long characterId, string message, string ipAddress)
        {
            LogChatDTO log = new LogChatDTO
            {
                CharacterId = characterId,
                ChatMessage = message,
                IpAddress = ipAddress,
                ChatType = (byte) type,
                Timestamp = DateTime.Now
            };
            ChatLogList.Add(log);
            if (ChatLogList.Count != 500)
            {
                return;
            }

            DaoFactory.LogChatDao.InsertOrUpdateList(ref ChatLogList);
            ChatLogList.Clear();
        }

        public void InsertQuestLog(long characterId, string ipAddress, long questId, DateTime lastDaily)
        {
            var log = new QuestLogDTO
            {
                CharacterId = characterId,
                IpAddress = ipAddress,
                QuestId = questId,
                LastDaily = lastDaily
            };
            QuestLogList.Add(log);
            if (QuestLogList.Count != 500)
            {
                return;
            }
            DaoFactory.QuestLogDao.InsertOrUpdateList(ref QuestLogList);
            QuestLogList.Clear();
        }

        public void InsertRaidLog(long characterId, long raidId, DateTime time)
        {
            var log = new RaidLogDTO
            {
                CharacterId = characterId,
                RaidId = raidId,
                Time = time
            };
            RaidLogList.Add(log);
            if (RaidLogList.Count != 500)
            {
                return;
            }
            DaoFactory.RaidLogDao.InsertOrUpdateList(ref RaidLogList);
            RaidLogList.Clear();
        }

        public void InsertFamilyRaidLog(long familyId, long raidId, DateTime time)
        {
            var log = new RaidLogDTO
            {
                FamilyId = familyId,
                RaidId = raidId,
                Time = time
            };
            RaidLogList.Add(log);
            if (RaidLogList.Count != 500)
            {
                return;
            }
            DaoFactory.RaidLogDao.InsertOrUpdateList(ref RaidLogList);
            RaidLogList.Clear();
        }

        #region Singleton

        private static LogHelper _instance;

        public static LogHelper Instance
        {
            get { return _instance ?? (_instance = new LogHelper()); }
        }

        #endregion
    }
}