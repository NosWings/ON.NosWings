using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite73 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "IsPetAutoRelive", c => c.Boolean(false));
            AddColumn("dbo.Character", "IsPartnerAutoRelive", c => c.Boolean(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Character", "IsPartnerAutoRelive");
            DropColumn("dbo.Character", "IsPetAutoRelive");
        }
    }
}