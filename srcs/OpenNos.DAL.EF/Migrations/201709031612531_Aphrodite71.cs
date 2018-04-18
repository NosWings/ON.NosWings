using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite71 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RollGeneratedItem", "ItemGeneratedUpgrade", c => c.Byte(false));
        }

        public override void Down()
        {
            DropColumn("dbo.RollGeneratedItem", "ItemGeneratedUpgrade");
        }
    }
}