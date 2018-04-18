using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite86 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "SpecialData", c => c.Int());
        }

        public override void Down()
        {
            DropColumn("dbo.Quest", "SpecialData");
        }
    }
}