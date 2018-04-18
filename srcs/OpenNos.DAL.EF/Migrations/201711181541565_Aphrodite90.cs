using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite90 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CharacterQuest", "FourthObjective", c => c.Int(false));
            AddColumn("dbo.CharacterQuest", "FifthObjective", c => c.Int(false));
            DropColumn("dbo.Quest", "IsMainQuest");
        }

        public override void Down()
        {
            AddColumn("dbo.Quest", "IsMainQuest", c => c.Boolean(false));
            DropColumn("dbo.CharacterQuest", "FifthObjective");
            DropColumn("dbo.CharacterQuest", "FourthObjective");
        }
    }
}