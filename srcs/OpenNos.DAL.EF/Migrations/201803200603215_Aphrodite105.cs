using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite105 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RaidLog", "FamilyId", c => c.Long());
            AlterColumn("dbo.RaidLog", "CharacterId", c => c.Long());
        }

        public override void Down()
        {
            AlterColumn("dbo.RaidLog", "CharacterId", c => c.Long(false));
            DropColumn("dbo.RaidLog", "FamilyId");
        }
    }
}