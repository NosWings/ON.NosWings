using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite54 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.RollGeneratedItem",
                    c => new
                    {
                        RollGeneratedItemId = c.Short(false, true),
                        OriginalItemRare = c.Byte(false),
                        OriginalItemDesign = c.Short(false),
                        OriginalItemVNum = c.Short(false),
                        Probability = c.Short(false),
                        ItemGeneratedAmount = c.Byte(false),
                        ItemGeneratedVNum = c.Short(false),
                        IsRareRandom = c.Boolean(false),
                        MinimumOriginalItemRare = c.Byte(false),
                        MaximumOriginalItemRare = c.Byte(false)
                    })
                .PrimaryKey(t => t.RollGeneratedItemId)
                .ForeignKey("dbo.Item", t => t.ItemGeneratedVNum)
                .ForeignKey("dbo.Item", t => t.OriginalItemVNum)
                .Index(t => t.OriginalItemVNum)
                .Index(t => t.ItemGeneratedVNum);
        }

        public override void Down()
        {
            DropForeignKey("dbo.RollGeneratedItem", "OriginalItemVNum", "dbo.Item");
            DropForeignKey("dbo.RollGeneratedItem", "ItemGeneratedVNum", "dbo.Item");
            DropIndex("dbo.RollGeneratedItem", new[] { "ItemGeneratedVNum" });
            DropIndex("dbo.RollGeneratedItem", new[] { "OriginalItemVNum" });
            DropTable("dbo.RollGeneratedItem");
        }
    }
}