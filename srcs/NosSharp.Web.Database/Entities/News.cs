using System.ComponentModel.DataAnnotations;

namespace NosSharp.Web.Database.Entities
{
    public class News
    {
        public uint NewsId { get; set; }
        
        public string Title { get; set; }

        public string Description { get; set; }

        public string Link { get; set; }

        public string ImageLink { get; set; }
    }
}
