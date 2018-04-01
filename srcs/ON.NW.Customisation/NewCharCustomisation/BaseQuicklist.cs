using System.Collections.Generic;
using OpenNos.Data;

namespace ON.NW.Customisation.NewCharCustomisation
{
    public class BaseQuicklist
    {
        public BaseQuicklist()
        {
            Quicklist = new List<QuicklistEntryDTO>
            {
                new QuicklistEntryDTO
                {
                    CharacterId = 0,
                    Type = 1,
                    Slot = 1,
                    Pos = 1
                },
                new QuicklistEntryDTO
                {
                    CharacterId = 0,
                    Q2 = 1,
                    Slot = 2
                },
                new QuicklistEntryDTO
                {
                    CharacterId = 0,
                    Q2 = 8,
                    Type = 1,
                    Slot = 1,
                    Pos = 16
                },
                new QuicklistEntryDTO
                {
                    CharacterId = 0,
                    Q2 = 9,
                    Type = 1,
                    Slot = 3,
                    Pos = 1
                },
            };
        }

        public IEnumerable<QuicklistEntryDTO> Quicklist { get; set; }
    }
}