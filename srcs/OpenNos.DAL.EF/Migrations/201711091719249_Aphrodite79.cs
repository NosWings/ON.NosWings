using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite79 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Character", "Elo", c => c.Int(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Character", "Elo");
        }
    }
}