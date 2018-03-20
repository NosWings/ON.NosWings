namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite104 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RaidLog",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        CharacterId = c.Long(nullable: false),
                        RaidId = c.Long(nullable: false),
                        Time = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.RaidLog");
        }
    }
}
