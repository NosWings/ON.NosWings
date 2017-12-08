namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite97 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teleporter", "Type", c => c.Byte(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Teleporter", "Type");
        }
    }
}
