using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$AddQuest", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class AddQuestPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public short QuestId { get; set; }

        public static string ReturnHelp()
        {
            return "$AddQuest QUESTID";
        }

        #endregion
    }
}
