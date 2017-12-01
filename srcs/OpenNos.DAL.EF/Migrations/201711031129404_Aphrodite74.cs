namespace OpenNos.DAL.EF.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Aphrodite74 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CharacterQuest",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CharacterId = c.Long(nullable: false),
                        QuestId = c.Long(nullable: false),
                        FirstObjective = c.Int(),
                        SecondObjective = c.Int(),
                        ThirdObjective = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Character", t => t.CharacterId, cascadeDelete: true)
                .ForeignKey("dbo.Quest", t => t.QuestId, cascadeDelete: true)
                .Index(t => t.CharacterId)
                .Index(t => t.QuestId);
            
            CreateTable(
                "dbo.Quest",
                c => new
                    {
                        QuestId = c.Long(nullable: false, identity: true),
                        QuestType = c.Byte(nullable: false),
                        FirstData = c.Int(nullable: false),
                        SecondData = c.Int(),
                        ThirdData = c.Int(),
                        FirstObjective = c.Int(nullable: false),
                        SecondObjective = c.Int(),
                        ThirdObjective = c.Int(),
                        TargetMap = c.Short(),
                        TargetX = c.Short(),
                        TargetY = c.Short(),
                    })
                .PrimaryKey(t => t.QuestId);
            
            CreateTable(
                "dbo.QuestReward",
                c => new
                    {
                        QuestRewardId = c.Long(nullable: false, identity: true),
                        RewardType = c.Byte(nullable: false),
                        Data = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                        QuestId = c.Long(nullable: false),
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
