using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$Act4Percent", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class Act4PercentagePacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public int? Faction { get; set; }

        [PacketIndex(1)]
        public int? Percent { get; set; }

        public override string ToString()
        {
            return "Act4Connect Faction Percent";
        }

        #endregion
    }
}
