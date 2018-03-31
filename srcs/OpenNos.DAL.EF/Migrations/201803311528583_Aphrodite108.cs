namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite108 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CharacterHome",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CharacterId = c.Long(nullable: false),
                        Name = c.String(),
                        MapId = c.Short(nullable: false),
                        MapX = c.Short(nullable: false),
                        MapY = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.CharacterId, cascadeDelete: true)
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
