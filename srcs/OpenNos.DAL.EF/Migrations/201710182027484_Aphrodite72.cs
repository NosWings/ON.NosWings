using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite72 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.RollGeneratedItem", "MinimumOriginalItemRare", c => c.Short(false));
            AlterColumn("dbo.RollGeneratedItem", "MaximumOriginalItemRare", c => c.Short(false));
        }

        public override void Down()
        {
            AlterColumn("dbo.RollGeneratedItem", "MaximumOriginalItemRare", c => c.Byte(false));
            AlterColumn("dbo.RollGeneratedItem", "MinimumOriginalItemRare", c => c.Byte(false));
        }
    }
}