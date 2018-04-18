using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite68 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.MapType", "MapTypeId");
        }

        public override void Down()
        {
            AddColumn("dbo.MapType", "MapTypeId", c => c.Short(false));
        }
    }
}