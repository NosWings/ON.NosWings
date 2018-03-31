// NosSharp
// CharacterHome.cs

using System.ComponentModel.DataAnnotations.Schema;
using OpenNos.DAL.EF.Entities.Base;

namespace OpenNos.DAL.EF.Entities
{
    public class CharacterHome : SynchronizableBaseEntity
    {
        public virtual Character Character { get; set; }

        [ForeignKey(nameof(Character))]
        public long CharacterId { get; set; }

        public string Name { get; set; }

        public short MapId { get; set; }
        public short MapX { get; set; }
        public short MapY { get; set; }
    }
}