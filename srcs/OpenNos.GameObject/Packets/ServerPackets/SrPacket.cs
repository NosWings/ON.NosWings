using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    [PacketHeader("sr")]
    public class SrPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int CastingId { get; set; }
    }
}