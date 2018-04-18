using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite65 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Character", "Faction", c => c.Byte(false));
        }

        public override void Down()
        {
            AlterColumn("dbo.Character", "Faction", c => c.Int(false));
        }
    }
}