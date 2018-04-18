using System;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class QuestLogDTO : MappingBaseDTO
    {
        public long CharacterId { get; set; }

        public long QuestId { get; set; }

        public string IpAddress { get; set; }

        public DateTime? LastDaily { get; set; }
    }
}