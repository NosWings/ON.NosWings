using System.Collections.Generic;
using NosSharp.Enums;

namespace ON.NW.Customisation.NewCharCustomisation
{
    public class BaseInventory
    {
        public BaseInventory() => Items = new List<StartupInventoryItem>
        {
            new StartupInventoryItem
            {
                InventoryType = InventoryType.Etc,
                Quantity = 10,
                Vnum = 2024
            },
            new StartupInventoryItem
            {
                Vnum = 2081,
                Quantity = 1,
                InventoryType = InventoryType.Etc
            },
            new StartupInventoryItem
            {
                Vnum = 1907,
                Quantity = 1,
                InventoryType = InventoryType.Main
            }
        };

        public ICollection<StartupInventoryItem> Items { get; set; }

        public class StartupInventoryItem
        {
            public short Vnum { get; set; }
            public ushort Quantity { get; set; }
            public InventoryType InventoryType { get; set; }
        }
    }
}