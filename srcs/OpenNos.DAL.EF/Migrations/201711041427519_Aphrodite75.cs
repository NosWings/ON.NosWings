using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite75 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "NextQuestId", c => c.Long());
            AlterColumn("dbo.CharacterQuest", "FirstObjective", c => c.Int(false));
            AlterColumn("dbo.CharacterQuest", "SecondObjective", c => c.Int(false));
            AlterColumn("dbo.CharacterQuest", "ThirdObjective", c => c.Int(false));
        }

        public override void Down()
        {
            AlterColumn("dbo.CharacterQuest", "ThirdObjective", c => c.Int());
            AlterColumn("dbo.CharacterQuest", "SecondObjective", c => c.Int());
            AlterColumn("dbo.CharacterQuest", "FirstObjective", c => c.Int());
            DropColumn("dbo.Quest", "NextQuestId");
        }
    }
}