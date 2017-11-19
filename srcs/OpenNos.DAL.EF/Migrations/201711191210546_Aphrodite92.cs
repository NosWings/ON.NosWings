namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite92 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest");
            DropIndex("dbo.CharacterQuest", new[] { "QuestId" });
            DropTable("dbo.Quest");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Quest",
                c => new
                    {
                        QuestId = c.Long(nullable: false, identity: true),
                        QuestType = c.Int(nullable: false),
                        FirstData = c.Int(nullable: false),
                        SecondData = c.Int(),
                        ThirdData = c.Int(),
                        FourthData = c.Int(),
                        FifthData = c.Int(),
                        SpecialData = c.Int(),
                        FirstSpecialData = c.Int(),
                        SecondSpecialData = c.Int(),
                        ThirdSpecialData = c.Int(),
                        FourthSpecialData = c.Int(),
                        FifthSpecialData = c.Int(),
                        FirstObjective = c.Int(nullable: false),
                        SecondObjective = c.Int(),
                        ThirdObjective = c.Int(),
                        FourthObjective = c.Int(),
                        FifthObjective = c.Int(),
                        TargetMap = c.Short(),
                        TargetX = c.Short(),
                        TargetY = c.Short(),
                        NextQuestId = c.Long(),
                        InfoId = c.Int(nullable: false),
                        IsDaily = c.Boolean(nullable: false),
                        EndDialogId = c.Int(),
                        StartDialogId = c.Int(),
                        LevelMin = c.Byte(nullable: false),
                        LevelMax = c.Byte(nullable: false),
                    })
                .PrimaryKey(t => t.QuestId);
            
            CreateIndex("dbo.CharacterQuest", "QuestId");
            AddForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest", "QuestId", cascadeDelete: true);
        }
    }
}
