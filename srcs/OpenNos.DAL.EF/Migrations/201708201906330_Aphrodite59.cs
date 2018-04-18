using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite59 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.LogChat",
                    c => new
                    {
                        LogId = c.Long(false, true),
                        CharacterId = c.Long(),
                        ChatType = c.Byte(false),
                        ChatMessage = c.String(maxLength: 255),
                        IpAddress = c.String(maxLength: 255),
                        Timestamp = c.DateTime(false)
                    })
                .PrimaryKey(t => t.LogId)
                .ForeignKey("dbo.Character", t => t.CharacterId)
                .Index(t => t.CharacterId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.LogChat", "CharacterId", "dbo.Character");
            DropIndex("dbo.LogChat", new[] { "CharacterId" });
            DropTable("dbo.LogChat");
        }
    }
}