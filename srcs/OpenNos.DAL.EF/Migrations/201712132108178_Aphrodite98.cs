using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite98 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MapNpc", "IsHostile", c => c.Boolean(false));
        }

        public override void Down()
        {
            DropColumn("dbo.MapNpc", "IsHostile");
        }
    }
}