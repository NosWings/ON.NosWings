using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenNos.DAL.EF.Entities
{
    public class QuestLog
    {
        [Key]
        public long Id { get; set; }

        public long CharacterId { get; set; }

        public long QuestId { get; set; }

        public string IpAddress { get; set; }
    }
}