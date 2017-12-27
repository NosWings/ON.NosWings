using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$Lobby", Authority = AuthorityType.GameMaster)]
    public class LobbyPacket : PacketDefinition
    {
        #region Methods

        public static string ReturnHelp()
        {
            return "$Lobby";
        }

        #endregion
    }
}