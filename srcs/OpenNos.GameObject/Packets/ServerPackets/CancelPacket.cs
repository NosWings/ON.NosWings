using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    [PacketHeader("cancel")]
    public class CancelPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public CancelType Type { get; set; }

        [PacketIndex(1)]
        public int TargetId { get; set; }
    }
}