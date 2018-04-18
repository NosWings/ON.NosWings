using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite101 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Recipe", "ProduceItemVNum", c => c.Short(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Recipe", "ProduceItemVNum");
        }
    }
}