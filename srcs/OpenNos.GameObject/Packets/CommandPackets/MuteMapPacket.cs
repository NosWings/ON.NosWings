using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$MuteMap", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class MuteMapPacket : PacketDefinition
    {
        #region Properties

        public static string ReturnHelp()
        {
            return "$MuteMap";
        }

        #endregion
    }
}