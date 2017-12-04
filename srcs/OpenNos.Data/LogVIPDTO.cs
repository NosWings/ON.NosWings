using System;
using System.ComponentModel.DataAnnotations;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class LogVIPDTO : MappingBaseDTO
    {
        [Key]
        public long LogId { get; set; }

        public long? AccountId { get; set; }

        public DateTime Timestamp { get; set; }

        public string VipPack { get; set; }
    }
}