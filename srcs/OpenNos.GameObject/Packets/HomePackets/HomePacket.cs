using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.HomePackets
{
    [PacketHeader("$Home", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class HomePacket
    {
        public string Name { get; set; }
    }
}