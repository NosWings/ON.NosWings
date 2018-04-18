using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite87 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quest", "FourthData", c => c.Int());
            AddColumn("dbo.Quest", "FifthData", c => c.Int());
            AddColumn("dbo.Quest", "FirstSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "SecondSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "ThirdSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FourthSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FifthSpecialData", c => c.Int());
            AddColumn("dbo.Quest", "FourthObjective", c => c.Int());
            AddColumn("dbo.Quest", "FifthObjective", c => c.Int());
            DropColumn("dbo.Quest", "SpecialData");
        }

        public override void Down()
        {
            AddColumn("dbo.Quest", "SpecialData", c => c.Int());
            DropColumn("dbo.Quest", "FifthObjective");
            DropColumn("dbo.Quest", "FourthObjective");
            DropColumn("dbo.Quest", "FifthSpecialData");
            DropColumn("dbo.Quest", "FourthSpecialData");
            DropColumn("dbo.Quest", "ThirdSpecialData");
            DropColumn("dbo.Quest", "SecondSpecialData");
            DropColumn("dbo.Quest", "FirstSpecialData");
            DropColumn("dbo.Quest", "FifthData");
            DropColumn("dbo.Quest", "FourthData");
        }
    }
}