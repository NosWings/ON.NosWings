using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite81 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestReward", "Design", c => c.Byte(false));
            AddColumn("dbo.QuestReward", "Rarity", c => c.Byte(false));
            AddColumn("dbo.QuestReward", "Upgrade", c => c.Byte(false));
        }

        public override void Down()
        {
            DropColumn("dbo.QuestReward", "Upgrade");
            DropColumn("dbo.QuestReward", "Rarity");
            DropColumn("dbo.QuestReward", "Design");
        }
    }
}