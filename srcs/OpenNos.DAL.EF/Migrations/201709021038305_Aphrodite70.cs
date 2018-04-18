using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite70 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BCard", "IsLevelDivided", c => c.Boolean(false));
            AddColumn("dbo.RollGeneratedItem", "IsSuperReward", c => c.Boolean(false));
        }

        public override void Down()
        {
            DropColumn("dbo.RollGeneratedItem", "IsSuperReward");
            DropColumn("dbo.BCard", "IsLevelDivided");
        }
    }
}