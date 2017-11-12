namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite83 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "IsPercent", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "IsPercent");
        }
    }
}
