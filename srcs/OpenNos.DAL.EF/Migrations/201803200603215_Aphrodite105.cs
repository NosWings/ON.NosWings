namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite105 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RaidLog", "FamilyId", c => c.Long());
            AlterColumn("dbo.RaidLog", "CharacterId", c => c.Long());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RaidLog", "CharacterId", c => c.Long(nullable: false));
            DropColumn("dbo.RaidLog", "FamilyId");
        }
    }
}
