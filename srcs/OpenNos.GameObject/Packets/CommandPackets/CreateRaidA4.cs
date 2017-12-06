using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$CreateRaid", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class CreateRaidPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string FactionType { get; set; }

        [PacketIndex(1)]
        public string RaidType { get; set; }

        public override string ToString()
        {
            return "CreateRaid faction raidType";
        }

        #endregion
    }
}
