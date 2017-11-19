using System;
using System.ComponentModel.DataAnnotations;

namespace OpenNos.Data
{
    public class LogCommandsDTO : MappingBaseDTO
    {
        [Key]
        public long CommandId { get; set; }

        public long? CharacterId { get; set; }

        public string Command { get; set; }

        public string Data { get; set; }

        [MaxLength(255)]
        public string IpAddress { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
