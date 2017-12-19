using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NosSharp.Web.Database.Entities
{
    public class ShopPack
    {
        public uint ShopPackId { get; set; }

        public string Title { get; set; }

        public string ImageLink { get; set; }

        public string Description { get; set; }

        public ushort Price { get; set; }

        public virtual ICollection<ShopPackItem> PackItems { get; set; }
    }
}
