using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF.Entities
{
    public class RaidLog
    {
        [Key]
        public long Id { get; set; }

        public long? CharacterId { get; set; }

        public long? FamilyId { get; set; }

        public long RaidId { get; set; }

        public DateTime Time { get; set; }
    }
}