namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite90 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CharacterQuest", "FourthObjective", c => c.Int(nullable: false));
            AddColumn("dbo.CharacterQuest", "FifthObjective", c => c.Int(nullable: false));
            DropColumn("dbo.Quest", "IsMainQuest");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Quest", "IsMainQuest", c => c.Boolean(nullable: false));
            DropColumn("dbo.CharacterQuest", "FifthObjective");
            DropColumn("dbo.CharacterQuest", "FourthObjective");
        }
    }
}
