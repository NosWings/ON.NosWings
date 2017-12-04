using OpenNos.Core.Serializing;
using OpenNos.Domain;

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
