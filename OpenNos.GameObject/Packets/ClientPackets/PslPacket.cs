using OpenNos.Core;

namespace OpenNos.GameObject.Packets.ClientPackets
{
    [PacketHeader("psl")]
    public class PslPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int Type { get; set; }
    }
}
