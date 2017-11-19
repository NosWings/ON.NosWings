using OpenNos.Core;
using OpenNos.Domain;

namespace OpenNos.GameObject.CommandPackets
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
