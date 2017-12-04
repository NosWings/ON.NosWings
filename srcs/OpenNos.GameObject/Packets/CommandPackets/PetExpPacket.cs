using OpenNos.Core.Serializing;
using OpenNos.Domain;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$PetExp", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class PetExpPacket : PacketDefinition
    {
        [PacketIndex(0)]
        public int Amount { get; set; }

        public static string ReturnHelp()
        {
            return "PetExp AMOUNT";
        }
    }
}
