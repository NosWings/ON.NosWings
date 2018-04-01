using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ServerPackets
{
    [PacketHeader("cancel")]
    public class CancelPacket : PacketDefinition
    {
        public CancelType Type { get; set; }

        public int TargetId { get; set; }
    }
}