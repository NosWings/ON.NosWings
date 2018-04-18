using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite9 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            DropForeignKey("dbo.StaticBonus", "CharacterId", "dbo.Character");
            DropIndex("dbo.StaticBonus", new[] { "CharacterId" });
            DropTable("dbo.StaticBonus");
        }

        public override void Up()
        {
            CreateTable(
                    "dbo.StaticBonus",
                    c => new
                    {
                        StaticBonusId = c.Long(false, true),
                        CharacterId = c.Long(false),
                        DateEnd = c.DateTime(false),
                        StaticBonusType = c.Byte(false)
                    })
                .PrimaryKey(t => t.StaticBonusId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);
        }

        #endregion
    }
}