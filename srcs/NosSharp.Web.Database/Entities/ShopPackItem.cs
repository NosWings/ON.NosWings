using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.Web.Database.Entities
{
    public class ShopPackItem
    {
        public uint ShopPackItemId { get; set; }

        public short VNum { get; set; }

        public uint Quantity { get; set; }

        public virtual ShopPack ShopPack { get; set; }
    }
}
