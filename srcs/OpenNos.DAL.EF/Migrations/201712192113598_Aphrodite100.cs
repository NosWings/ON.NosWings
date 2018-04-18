using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite100 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.QuestObjective",
                    c => new
                    {
                        QuestObjectiveId = c.Int(false, true),
                        QuestId = c.Int(false),
                        Data = c.Int(),
                        Objective = c.Int(),
                        SpecialData = c.Int(),
                        DropRate = c.Int(),
                        ObjectiveIndex = c.Byte(false)
                    })
                .PrimaryKey(t => t.QuestObjectiveId);

            DropColumn("dbo.Quest", "FirstData");
            DropColumn("dbo.Quest", "FirstObjective");
            DropColumn("dbo.Quest", "FirstSpecialData");
            DropColumn("dbo.Quest", "SecondData");
            DropColumn("dbo.Quest", "SecondObjective");
            DropColumn("dbo.Quest", "SecondSpecialData");
            DropColumn("dbo.Quest", "ThirdData");
            DropColumn("dbo.Quest", "ThirdObjective");
            DropColumn("dbo.Quest", "ThirdSpecialData");
            DropColumn("dbo.Quest", "FourthData");
            DropColumn("dbo.Quest", "FourthObjective");
            DropColumn("dbo.Quest", "FourthSpecialData");
            DropColumn("dbo.Quest", "FifthData");
            DropColumn("dbo.Quest", "FifthObjective");
            DropColumn("dbo.Quest", "FifthSpecialData");
            DropColumn("dbo.Quest", "SpecialData");
        }

        public override void Down()
        {
            AddColumn("dbo.Quest", "SpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FifthSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FifthObjective", c => c.Int());
            AddColumn("dbo.Quest", "FifthData", c => c.Int());
            AddColumn("dbo.Quest", "FourthSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FourthObjective", c => c.Int());
            AddColumn("dbo.Quest", "FourthData", c => c.Int());
            AddColumn("dbo.Quest", "ThirdSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "ThirdObjective", c => c.Int());
            AddColumn("dbo.Quest", "ThirdData", c => c.Int());
            AddColumn("dbo.Quest", "SecondSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "SecondObjective", c => c.Int());
            AddColumn("dbo.Quest", "SecondData", c => c.Int());
            AddColumn("dbo.Quest", "FirstSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FirstObjective", c => c.Int(false));
            AddColumn("dbo.Quest", "FirstData", c => c.Int(false));
            DropTable("dbo.QuestObjective");
        }
    }
}