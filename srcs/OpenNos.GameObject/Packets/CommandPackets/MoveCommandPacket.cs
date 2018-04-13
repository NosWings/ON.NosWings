using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$Move", PassNonParseablePacket = true, Authority = AuthorityType.User)]
    public class MoveCommandPacket : PacketDefinition
    {
        public static string ReturnHelp()
        {
            return "$Move";
        }

        public override string ToString()
        {
            return "$Move";
        }
    }
}