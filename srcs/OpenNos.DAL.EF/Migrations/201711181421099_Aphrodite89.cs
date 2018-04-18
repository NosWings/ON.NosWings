using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite89 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "IsMainQuest", c => c.Boolean(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Quest", "IsMainQuest");
        }
    }
}