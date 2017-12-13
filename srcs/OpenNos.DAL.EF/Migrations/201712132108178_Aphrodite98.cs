namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite98 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MapNpc", "IsHostile", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MapNpc", "IsHostile");
        }
    }
}
