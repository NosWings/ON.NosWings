using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF.Entities
{
    public class QuestLog
    {
        [Key]
        public long Id { get; set; }

        public long CharacterId { get; set; }

        public long QuestId { get; set; }

        public string IpAddress { get; set; }

        public DateTime? LastDaily { get; set; }
    }
}