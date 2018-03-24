namespace OpenNos.DAL.EF.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Aphrodite106 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.BCard");
            AlterColumn("dbo.BCard", "BCardId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.BCard", "BCardId");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.BCard");
            AlterColumn("dbo.BCard", "BCardId", c => c.Short(nullable: false, identity: true));
            AddPrimaryKey("dbo.BCard", "BCardId");
        }
    }
}
