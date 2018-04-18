using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite97 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teleporter", "Type", c => c.Byte(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Teleporter", "Type");
        }
    }
}