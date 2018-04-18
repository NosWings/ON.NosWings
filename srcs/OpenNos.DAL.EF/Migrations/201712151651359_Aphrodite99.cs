using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite99 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.MapNpc", "IsHostile");
        }

        public override void Down()
        {
            AddColumn("dbo.MapNpc", "IsHostile", c => c.Boolean(false));
        }
    }
}