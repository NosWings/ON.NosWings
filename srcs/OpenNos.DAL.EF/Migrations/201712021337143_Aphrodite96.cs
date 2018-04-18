using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite96 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CharacterQuest", "FifthObjective", c => c.Int(false));
        }

        public override void Down()
        {
            DropColumn("dbo.CharacterQuest", "FifthObjective");
        }
    }
}