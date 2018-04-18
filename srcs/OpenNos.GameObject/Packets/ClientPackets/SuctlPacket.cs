using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.ClientPackets
{
    [PacketHeader("suctl")]
    public class SuctlPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int CastId { get; set; }

        [PacketIndex(1)]
        public int Unknown2 { get; set; }

        [PacketIndex(2)]
        public int MateTransportId { get; set; }

        [PacketIndex(3)]
        public UserType TargetType { get; set; }

        [PacketIndex(4)]
        public int TargetId { get; set; }

        public override string ToString() => $"{CastId} {Unknown2} {MateTransportId} {TargetType} {TargetId}";
    }
}