using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite62 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItemInstance", "ShellRarity", c => c.Byte());
        }

        public override void Down()
        {
            DropColumn("dbo.ItemInstance", "ShellRarity");
        }
    }
}