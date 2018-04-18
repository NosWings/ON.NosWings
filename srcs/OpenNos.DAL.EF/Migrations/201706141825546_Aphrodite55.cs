using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite55 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.RollGeneratedItem", "OriginalItemRare");
        }

        public override void Down()
        {
            AddColumn("dbo.RollGeneratedItem", "OriginalItemRare", c => c.Byte(false));
        }
    }
}