using System;
using OpenNos.Domain;
using NosSharp.Logs;

namespace OpenNos.GameObject.Logs.Classes
{
    public class ChatLog : AbstractLog
    {
        public ChatLog() : base("ChatLogs")
        {
        }

        /// <summary>
        /// Character Sender
        /// </summary>
        public CharacterLog Sender { get; set; }

        /// <summary>
        /// Sent message channelType
        /// </summary>
        public ChatType ChatType { get; set; }

        /// <summary>
        /// Message that has been sent
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Target AccountId if it exist
        /// </summary>
        public long? TargetCharacterAccountId { get; set; }

        /// <summary>
        /// Target Character Name if it exist
        /// </summary>
        public string TargetCharacterName { get; set; }

        /// <summary>
        /// Target CharacterId Name if it exist
        /// </summary>
        public int? TargetCharacterId { get; set; }

        /// <summary>
        /// Channel from where the ChatLog has been created
        /// </summary>
        public int ChannelId { get; set; }
    }
}