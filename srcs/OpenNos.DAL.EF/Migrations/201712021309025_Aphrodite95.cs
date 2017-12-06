namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite95 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "FifthData", c => c.Int());
            AddColumn("dbo.Quest", "FifthObjective", c => c.Int());
            AddColumn("dbo.Quest", "FifthSpecialData", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quest", "FifthSpecialData");
            DropColumn("dbo.Quest", "FifthObjective");
            DropColumn("dbo.Quest", "FifthData");
        }
    }
}
