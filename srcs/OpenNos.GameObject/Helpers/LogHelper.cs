using System;
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
            DaoFactory.LogCommandsDao.InsertOrUpdate(ref command);
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
            DaoFactory.LogChatDao.InsertOrUpdate(ref log);
        }

        public void InsertQuestLog(long characterId, string ipAddress, long questId)
        {
            var log = new QuestLogDTO
            {
                CharacterId = characterId,
                IpAddress = ipAddress,
                QuestId = questId
            };
            DaoFactory.QuestLogDao.InsertOrUpdate(ref log);
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