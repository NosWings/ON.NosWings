namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite96 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CharacterQuest", "FifthObjective", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CharacterQuest", "FifthObjective");
        }
    }
}
