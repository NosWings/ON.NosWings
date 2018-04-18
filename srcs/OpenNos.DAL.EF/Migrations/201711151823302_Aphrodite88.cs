using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite88 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CharacterQuest", "IsMainQuest", c => c.Boolean(false));
            AlterColumn("dbo.Quest", "QuestType", c => c.Int(false));
        }

        public override void Down()
        {
            AlterColumn("dbo.Quest", "QuestType", c => c.Byte(false));
            DropColumn("dbo.CharacterQuest", "IsMainQuest");
        }
    }
}