using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
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
                        QuestId = c.Long(false, true),
                        QuestType = c.Int(false),
                        FirstData = c.Int(false),
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
                        FirstObjective = c.Int(false),
                        SecondObjective = c.Int(),
                        ThirdObjective = c.Int(),
                        FourthObjective = c.Int(),
                        FifthObjective = c.Int(),
                        TargetMap = c.Short(),
                        TargetX = c.Short(),
                        TargetY = c.Short(),
                        NextQuestId = c.Long(),
                        InfoId = c.Int(false),
                        IsDaily = c.Boolean(false),
                        EndDialogId = c.Int(),
                        StartDialogId = c.Int(),
                        LevelMin = c.Byte(false),
                        LevelMax = c.Byte(false)
                    })
                .PrimaryKey(t => t.QuestId);

            CreateIndex("dbo.CharacterQuest", "QuestId");
            AddForeignKey("dbo.CharacterQuest", "QuestId", "dbo.Quest", "QuestId", true);
        }
    }
}