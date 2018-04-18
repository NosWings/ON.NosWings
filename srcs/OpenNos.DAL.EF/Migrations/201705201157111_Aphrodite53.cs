using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite53 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StaticBuff", "CardId", c => c.Short(false));
            CreateIndex("dbo.StaticBuff", "CardId");
            AddForeignKey("dbo.StaticBuff", "CardId", "dbo.Card", "CardId");
            DropColumn("dbo.StaticBuff", "EffectId");
        }

        public override void Down()
        {
            AddColumn("dbo.StaticBuff", "EffectId", c => c.Int(false));
            DropForeignKey("dbo.StaticBuff", "CardId", "dbo.Card");
            DropIndex("dbo.StaticBuff", new[] { "CardId" });
            DropColumn("dbo.StaticBuff", "CardId");
        }
    }
}