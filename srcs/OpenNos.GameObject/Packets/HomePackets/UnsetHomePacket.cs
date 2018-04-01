using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.HomePackets
{
    [PacketHeader("$UnsetHome", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class UnsetHomePacket
    {
        public string Name { get; set; }
    }
}