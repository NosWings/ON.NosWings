using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite106 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.BCard");
            AlterColumn("dbo.BCard", "BCardId", c => c.Int(false, true));
            AddPrimaryKey("dbo.BCard", "BCardId");
        }

        public override void Down()
        {
            DropPrimaryKey("dbo.BCard");
            AlterColumn("dbo.BCard", "BCardId", c => c.Short(false, true));
            AddPrimaryKey("dbo.BCard", "BCardId");
        }
    }
}