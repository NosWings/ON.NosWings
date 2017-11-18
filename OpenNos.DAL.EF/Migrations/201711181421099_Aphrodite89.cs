namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite89 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "IsMainQuest", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quest", "IsMainQuest");
        }
    }
}
