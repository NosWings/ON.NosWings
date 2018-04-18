using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite76 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "InfoId", c => c.Int(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Quest", "InfoId");
        }
    }
}