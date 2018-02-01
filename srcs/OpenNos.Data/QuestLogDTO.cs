using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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