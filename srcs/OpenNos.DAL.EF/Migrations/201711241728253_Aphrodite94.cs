namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite94 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.CharacterQuest", "FifthObjective");
            DropColumn("dbo.Quest", "FifthData");
            DropColumn("dbo.Quest", "FifthSpecialData");
            DropColumn("dbo.Quest", "FifthObjective");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Quest", "FifthObjective", c => c.Int());
            AddColumn("dbo.Quest", "FifthSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FifthData", c => c.Int());
            AddColumn("dbo.CharacterQuest", "FifthObjective", c => c.Int(nullable: false));
        }
    }
}
