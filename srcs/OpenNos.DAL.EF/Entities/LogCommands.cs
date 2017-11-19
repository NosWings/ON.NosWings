using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF.Entities
{
    public class LogCommands
    {
        [Key]
        public long CommandId { get; set; }

        public virtual Character Character { get; set; }

        public long? CharacterId { get; set; }

        public string Command { get; set; }

        public string Data { get; set; }

        [MaxLength(255)]
        public string IpAddress { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
