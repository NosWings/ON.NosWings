using NosSharp.Enums;
using OpenNos.Core.Serializing;

namespace OpenNos.GameObject.Packets.CommandPackets
{
    [PacketHeader("$GenerateShell", PassNonParseablePacket = true, Authority = AuthorityType.GameMaster)]
    public class GenerateShellPacket : PacketDefinition
    {
        #region Properties

        [PacketIndex(0)]
        public string ItemType { get; set; }

        [PacketIndex(1)]
        public byte? Type { get; set; }

        [PacketIndex(2)]
        public byte? Value { get; set; }

        public override string ToString() => "GenerateShell ItemType Type Value\nItemTypes : Weapon, SecondaryWeapon, Armor";

        #endregion
    }
}