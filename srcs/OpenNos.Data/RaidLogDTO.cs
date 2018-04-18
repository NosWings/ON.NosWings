using System;
using OpenNos.Data.Base;

namespace OpenNos.Data
{
    public class RaidLogDTO : MappingBaseDTO
    {
        public long? CharacterId { get; set; }

        public long? FamilyId { get; set; }

        public long RaidId { get; set; }

        public DateTime Time { get; set; }
    }
}