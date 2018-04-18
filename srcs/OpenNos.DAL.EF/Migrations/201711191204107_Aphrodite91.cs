using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite91 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest");
            DropPrimaryKey("dbo.Quest");
            AlterColumn("dbo.Quest", "QuestId", c => c.Long(false, true));
            AddPrimaryKey("dbo.Quest", "QuestId");
            AddForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest", "QuestId", true);
        }

        public override void Down()
        {
            DropForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest");
            DropPrimaryKey("dbo.Quest");
            AlterColumn("dbo.Quest", "QuestId", c => c.Long(false));
            AddPrimaryKey("dbo.Quest", "QuestId");
            AddForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest", "QuestId", true);
        }
    }
}