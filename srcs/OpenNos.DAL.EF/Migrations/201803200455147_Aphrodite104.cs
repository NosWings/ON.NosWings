using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite104 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.RaidLog",
                    c => new
                    {
                        Id = c.Long(false, true),
                        CharacterId = c.Long(false),
                        RaidId = c.Long(false),
                        Time = c.DateTime(false)
                    })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.RaidLog");
        }
    }
}