using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite46 : DbMigration
    {
        #region Methods

        public override void Down()
        {
            CreateTable(
                    "dbo.TimeSpace",
                    c => new
                    {
                        TimespaceId = c.Short(false, true),
                        MapId = c.Short(false),
                        PositionX = c.Short(false),
                        PositionY = c.Short(false),
                        Winner = c.String(maxLength: 255),
                        Script = c.String(),
                        WinnerScore = c.Int(false)
                    })
                .PrimaryKey(t => t.TimespaceId);

            DropForeignKey("dbo.ScriptedInstance", "MapId", "dbo.Map");
            DropIndex("dbo.ScriptedInstance", new[] { "MapId" });
            DropTable("dbo.ScriptedInstance");
            CreateIndex("dbo.TimeSpace", "MapId");
            AddForeignKey("dbo.TimeSpace", "MapId", "dbo.Map", "MapId");
        }

        public override void Up()
        {
            DropForeignKey("dbo.TimeSpace", "MapId", "dbo.Map");
            DropIndex("dbo.TimeSpace", new[] { "MapId" });
            CreateTable(
                    "dbo.ScriptedInstance",
                    c => new
                    {
                        ScriptedInstanceId = c.Short(false, true),
                        Type = c.Byte(false),
                        MapId = c.Short(false),
                        PositionX = c.Short(false),
                        PositionY = c.Short(false),
                        Winner = c.String(maxLength: 255),
                        Script = c.String(),
                        WinnerScore = c.Int(false)
                    })
                .PrimaryKey(t => t.ScriptedInstanceId)
                .ForeignKey("dbo.Map", t => t.MapId)
                .Index(t => t.MapId);

            DropTable("dbo.TimeSpace");
        }

        #endregion
    }
}