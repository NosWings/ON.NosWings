using System.Data.Entity.Migrations;

namespace OpenNos.DAL.EF.Migrations
{
    public partial class Aphrodite58 : DbMigration
    {
        public override void Up()
        {
            RenameTable("dbo.CellonOption", "EquipmentOption");
            AddColumn("dbo.Family", "FamilyFaction", c => c.Byte(false));
        }

        public override void Down()
        {
            DropColumn("dbo.Family", "FamilyFaction");
            RenameTable("dbo.EquipmentOption", "CellonOption");
        }
    }
}