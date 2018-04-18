using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite57 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BCard", "IsLevelScaled", c => c.Boolean(false));
        }

        public override void Down()
        {
            DropColumn("dbo.BCard", "IsLevelScaled");
        }
    }
}