using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite49 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Card", "Delay", c => c.Int(false));
            AddColumn("dbo.Card", "TimeoutBuff", c => c.Short(false));
            AddColumn("dbo.Card", "TimeoutBuffChance", c => c.Byte(false));
            AddColumn("dbo.Card", "BuffType", c => c.Byte(false));
            DropColumn("dbo.Card", "Period");
        }

        public override void Down()
        {
            AddColumn("dbo.Card", "Period", c => c.Short(false));
            DropColumn("dbo.Card", "BuffType");
            DropColumn("dbo.Card", "TimeoutBuffChance");
            DropColumn("dbo.Card", "TimeoutBuff");
            DropColumn("dbo.Card", "Delay");
        }
    }
}