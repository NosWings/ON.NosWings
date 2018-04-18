using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite52 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BCard", "IsDelayed", c => c.Boolean(false));
            AddColumn("dbo.BCard", "Delay", c => c.Short(false));
            DropColumn("dbo.BCard", "Delayed");
        }

        public override void Down()
        {
            AddColumn("dbo.BCard", "Delayed", c => c.Boolean(false));
            DropColumn("dbo.BCard", "Delay");
            DropColumn("dbo.BCard", "IsDelayed");
        }
    }
}