using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite83 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "IsPercent", c => c.Boolean(false));
        }

        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "IsPercent");
        }
    }
}