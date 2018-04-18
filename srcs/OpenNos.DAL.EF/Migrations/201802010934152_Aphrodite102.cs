using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite102 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.QuestLog",
                    c => new
                    {
                        Id = c.Long(false, true),
                        CharacterId = c.Long(false),
                        QuestId = c.Long(false),
                        IpAddress = c.String()
                    })
                .PrimaryKey(t => t.Id);
        }

        public override void Down()
        {
            DropTable("dbo.QuestLog");
        }
    }
}