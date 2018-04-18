using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite56 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BCard", "CastType", c => c.Byte(false));
            AddColumn("dbo.BCard", "ThirdData", c => c.Int(false));
            DropColumn("dbo.BCard", "IsDelayed");
            DropColumn("dbo.BCard", "Delay");
        }

        public override void Down()
        {
            AddColumn("dbo.BCard", "Delay", c => c.Short(false));
            AddColumn("dbo.BCard", "IsDelayed", c => c.Boolean(false));
            DropColumn("dbo.BCard", "ThirdData");
            DropColumn("dbo.BCard", "CastType");
        }
    }
}