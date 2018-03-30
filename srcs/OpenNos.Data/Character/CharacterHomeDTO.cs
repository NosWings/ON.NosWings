// NosSharp
// CharacterHomeDTO.cs

using System;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class CharacterHomeDto : SynchronizableBaseDTO
    {
        public Guid Id { get; set; }

        public long CharacterId { get; set; }

        public string Name { get; set; }

        public short MapId { get; set; }
        public short MapX { get; set; }
        public short MapY { get; set; }
    }
}