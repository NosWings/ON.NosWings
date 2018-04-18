using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite108 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.CharacterHome",
                    c => new
                    {
                        Id = c.Guid(false),
                        CharacterId = c.Long(false),
                        Name = c.String(),
                        MapId = c.Short(false),
                        MapX = c.Short(false),
                        MapY = c.Short(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.CharacterId, true)
                .Index(t => t.CharacterId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.CharacterHome", "CharacterId", "dbo.Character");
            DropIndex("dbo.CharacterHome", new[] { "CharacterId" });
            DropTable("dbo.CharacterHome");
        }
    }
}