using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite84 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "TakeDamages", c => c.Int(false));
            AddColumn("dbo.NpcMonster", "GiveDamagePercentage", c => c.Int(false));
        }

        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "GiveDamagePercentage");
            DropColumn("dbo.NpcMonster", "TakeDamages");
        }
    }
}