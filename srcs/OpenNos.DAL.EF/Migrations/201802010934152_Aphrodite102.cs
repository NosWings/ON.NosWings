namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite102 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuestLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CharacterId = c.Long(nullable: false),
                        QuestId = c.Long(nullable: false),
                        IpAddress = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.QuestLog");
        }
    }
}
