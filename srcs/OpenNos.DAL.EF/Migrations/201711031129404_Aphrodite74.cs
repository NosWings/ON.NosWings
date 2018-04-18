using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite74 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.CharacterQuest",
                    c => new
                    {
                        Id = c.Guid(false),
                        CharacterId = c.Long(false),
                        QuestId = c.Long(false),
                        FirstObjective = c.Int(),
                        SecondObjective = c.Int(),
                        ThirdObjective = c.Int()
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.CharacterId, true)
                .ForeignKey("dbo.Quest", t => t.QuestId, true)
                .Index(t => t.CharacterId)
                .Index(t => t.QuestId);

            CreateTable(
                    "dbo.Quest",
                    c => new
                    {
                        QuestId = c.Long(false, true),
                        QuestType = c.Byte(false),
                        FirstData = c.Int(false),
                        SecondData = c.Int(),
                        ThirdData = c.Int(),
                        FirstObjective = c.Int(false),
                        SecondObjective = c.Int(),
                        ThirdObjective = c.Int(),
                        TargetMap = c.Short(),
                        TargetX = c.Short(),
                        TargetY = c.Short()
                    })
                .PrimaryKey(t => t.QuestId);

            CreateTable(
                    "dbo.QuestReward",
                    c => new
                    {
                        QuestRewardId = c.Long(false, true),
                        RewardType = c.Byte(false),
                        Data = c.Int(false),
                        Amount = c.Int(false),
                        QuestId = c.Long(false)
                    })
                .PrimaryKey(t => t.QuestRewardId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest");
            DropForeignKey("dbo.CharacterQuest", "CharacterId", "dbo.Character");
            DropIndex("dbo.CharacterQuest", new[] { "QuestId" });
            DropIndex("dbo.CharacterQuest", new[] { "CharacterId" });
            DropTable("dbo.QuestReward");
            DropTable("dbo.Quest");
            DropTable("dbo.CharacterQuest");
        }
    }
}