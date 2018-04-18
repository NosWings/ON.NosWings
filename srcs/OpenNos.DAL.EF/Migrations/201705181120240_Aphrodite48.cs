using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite48 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.BCard",
                    c => new
                    {
                        BCardId = c.Short(false, true),
                        SubType = c.Byte(false),
                        Type = c.Byte(false),
                        FirstData = c.Int(false),
                        SecondData = c.Int(false),
                        CardId = c.Short(false),
                        Delayed = c.Boolean(false)
                    })
                .PrimaryKey(t => t.BCardId)
                .ForeignKey("dbo.Card", t => t.CardId)
                .Index(t => t.CardId);

            DropColumn("dbo.Card", "FirstData");
            DropColumn("dbo.Card", "SecondData");
            DropColumn("dbo.Card", "SubType");
            DropColumn("dbo.Card", "Type");
        }

        public override void Down()
        {
            AddColumn("dbo.Card", "Type", c => c.Short(false));
            AddColumn("dbo.Card", "SubType", c => c.Byte(false));
            AddColumn("dbo.Card", "SecondData", c => c.Int(false));
            AddColumn("dbo.Card", "FirstData", c => c.Int(false));
            DropForeignKey("dbo.BCard", "CardId", "dbo.Card");
            DropIndex("dbo.BCard", new[] { "CardId" });
            DropTable("dbo.BCard");
        }
    }
}