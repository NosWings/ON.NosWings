using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$Restart", PassNonParseablePacket = true, Authority = AuthorityType.Administrator)]
    public class RestartPacket : PacketDefinition
    {
        public static string ReturnHelp()
        {
            return "$Restart";
        }
    }
}