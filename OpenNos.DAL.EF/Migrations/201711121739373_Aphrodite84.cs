namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite84 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NpcMonster", "TakeDamages", c => c.Int(nullable: false));
            AddColumn("dbo.NpcMonster", "GiveDamagePercentage", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NpcMonster", "GiveDamagePercentage");
            DropColumn("dbo.NpcMonster", "TakeDamages");
        }
    }
}
