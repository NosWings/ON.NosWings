using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite64 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItemInstance", "CellonOptionId");
        }

        public override void Down()
        {
            AddColumn("dbo.ItemInstance", "CellonOptionId", c => c.Guid());
        }
    }
}